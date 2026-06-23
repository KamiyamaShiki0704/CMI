#include <Windows.h>

#include <array>
#include <cstdint>
#include <cwchar>
#include <cstring>
#include <cstdio>

static HMODULE g_module = nullptr;

static constexpr DWORD kBridgeMagic = 0x46494D43; // CMIF
static constexpr DWORD kBridgeVersion = 1;
static constexpr DWORD kBridgeSlotCount = 64;
static constexpr wchar_t kBridgeMappingName[] = L"Local\\CMI_Nightreign_FlagBridge";

struct FlagSlot
{
    volatile LONG flag_id;
    volatile LONG state;
    volatile LONG sequence;
    volatile LONG last_error;
};

struct FlagBridgeShared
{
    volatile DWORD magic;
    volatile DWORD version;
    volatile DWORD status;
    volatile DWORD slot_count;
    volatile unsigned long long event_flag_man;
    volatile DWORD last_error;
    volatile DWORD reserved;
    FlagSlot slots[kBridgeSlotCount];
};

struct SectionInfo
{
    const char* name;
    uint8_t* data;
    DWORD rva;
    DWORD size;
};

enum class InstructionKind
{
    LeaRcx,
    LeaR8,
    LeaR9,
    MovR9,
    MovEdx
};

struct Instruction
{
    InstructionKind kind;
    size_t pos;
};

struct Fd4Candidate
{
    size_t static_disp;
    size_t reflection_disp;
    size_t get_name_disp;
};

using GetSingletonName = const char* (__cdecl*)(const uint8_t*);

static void loader_directory(wchar_t* buffer, size_t count)
{
    GetModuleFileNameW(g_module, buffer, static_cast<DWORD>(count));
    wchar_t* slash = wcsrchr(buffer, L'\\');
    if (slash != nullptr)
    {
        *slash = L'\0';
    }
}

static bool current_process_is_supported_for_flag_bridge()
{
    wchar_t path[MAX_PATH] = {};
    GetModuleFileNameW(nullptr, path, MAX_PATH);
    wchar_t* name = wcsrchr(path, L'\\');
    name = name != nullptr ? name + 1 : path;

    return _wcsicmp(name, L"nightreign.exe") == 0 || _wcsicmp(name, L"eldenring.exe") == 0;
}

static void append_log(const wchar_t* message)
{
    wchar_t dir[MAX_PATH] = {};
    loader_directory(dir, MAX_PATH);

    wchar_t log_path[MAX_PATH] = {};
    swprintf_s(log_path, L"%s\\cmi_nightreign_loader.log", dir);

    FILE* file = nullptr;
    if (_wfopen_s(&file, log_path, L"a, ccs=UTF-8") != 0 || file == nullptr)
    {
        return;
    }

    fwprintf(file, L"%s\r\n", message);
    fclose(file);
}

template <typename T>
static bool safe_read(const T* ptr, T& value)
{
    __try
    {
        value = *ptr;
        return true;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static int32_t read_i32_unaligned(const uint8_t* ptr)
{
    int32_t value = 0;
    std::memcpy(&value, ptr, sizeof(value));
    return value;
}

static bool bytes_equal(const uint8_t* data, size_t size, size_t pos, const uint8_t* pattern, size_t pattern_size)
{
    return pos + pattern_size <= size && std::memcmp(data + pos, pattern, pattern_size) == 0;
}

static size_t instruction_next_pos(const Instruction& instruction)
{
    switch (instruction.kind)
    {
    case InstructionKind::MovEdx:
        return instruction.pos + 5;
    case InstructionKind::LeaRcx:
    case InstructionKind::LeaR8:
    case InstructionKind::LeaR9:
        return instruction.pos + 7;
    case InstructionKind::MovR9:
        return instruction.pos + 3;
    default:
        return instruction.pos;
    }
}

static bool append_instruction(const uint8_t* text, size_t size, Instruction* instructions, size_t& count)
{
    static constexpr uint8_t lea_rcx[] = { 0x48, 0x8D, 0x0D };
    static constexpr uint8_t lea_r8[] = { 0x4C, 0x8D, 0x05 };
    static constexpr uint8_t lea_r9[] = { 0x4C, 0x8D, 0x0D };
    static constexpr uint8_t mov_r9[] = { 0x4C, 0x8B, 0xC8 };

    size_t next = instruction_next_pos(instructions[count - 1]);
    if (next + 3 > size) return false;

    if (bytes_equal(text, size, next, lea_rcx, sizeof(lea_rcx)))
        instructions[count++] = { InstructionKind::LeaRcx, next };
    else if (bytes_equal(text, size, next, lea_r8, sizeof(lea_r8)))
        instructions[count++] = { InstructionKind::LeaR8, next };
    else if (bytes_equal(text, size, next, lea_r9, sizeof(lea_r9)))
        instructions[count++] = { InstructionKind::LeaR9, next };
    else if (bytes_equal(text, size, next, mov_r9, sizeof(mov_r9)))
        instructions[count++] = { InstructionKind::MovR9, next };
    else if (text[next] == 0xE8)
        return false;
    else
        return false;

    return true;
}

static bool prepend_instruction(const uint8_t* text, size_t size, Instruction* instructions, size_t& count)
{
    static constexpr uint8_t lea_rcx[] = { 0x48, 0x8D, 0x0D };
    static constexpr uint8_t lea_r8[] = { 0x4C, 0x8D, 0x05 };
    static constexpr uint8_t lea_r9[] = { 0x4C, 0x8D, 0x0D };
    static constexpr uint8_t mov_r9[] = { 0x4C, 0x8B, 0xC8 };

    if (instructions[0].pos < 7) return false;
    size_t prev = instructions[0].pos - 7;

    Instruction instruction = {};
    if (bytes_equal(text, size, prev, lea_rcx, sizeof(lea_rcx)))
        instruction = { InstructionKind::LeaRcx, prev };
    else if (bytes_equal(text, size, prev, lea_r8, sizeof(lea_r8)))
        instruction = { InstructionKind::LeaR8, prev };
    else if (bytes_equal(text, size, prev, lea_r9, sizeof(lea_r9)))
        instruction = { InstructionKind::LeaR9, prev };
    else if (prev + 7 <= size && std::memcmp(text + prev + 4, mov_r9, sizeof(mov_r9)) == 0)
        instruction = { InstructionKind::MovR9, prev + 4 };
    else
        return false;

    for (size_t i = count; i > 0; --i)
        instructions[i] = instructions[i - 1];
    instructions[0] = instruction;
    ++count;
    return true;
}

static bool conditional_jump_pos(const uint8_t* text, size_t size, size_t pos, size_t& jump_pos)
{
    if (pos >= 2 && text[pos - 2] == 0x75)
    {
        jump_pos = pos - 2;
        return true;
    }

    if (pos >= 6 && bytes_equal(text, size, pos - 6, reinterpret_cast<const uint8_t*>("\x0F\x85"), 2))
    {
        jump_pos = pos - 6;
        return true;
    }

    return false;
}

static bool try_fd4_candidate(const uint8_t* text, size_t size, size_t pos, Fd4Candidate& candidate)
{
    Instruction instructions[4] = { { InstructionKind::MovEdx, pos } };
    size_t count = 1;

    while (count < 4)
    {
        size_t before = count;
        if (!append_instruction(text, size, instructions, count))
            break;
        if (count == before)
            break;
    }

    while (count < 4)
    {
        if (!prepend_instruction(text, size, instructions, count))
            return false;
    }

    int mask = 0;
    for (size_t i = 0; i < 4; ++i)
    {
        switch (instructions[i].kind)
        {
        case InstructionKind::LeaRcx:
            mask |= 1;
            break;
        case InstructionKind::LeaR8:
            mask |= 2;
            break;
        case InstructionKind::LeaR9:
            mask |= 4;
            break;
        case InstructionKind::MovR9:
            mask |= 8;
            break;
        default:
            break;
        }
    }

    if (mask != 11)
        return false;

    for (size_t call_pad = 0; call_pad <= 1; ++call_pad)
    {
        if (instructions[0].pos < 5 + call_pad)
            continue;
        size_t call_pos = instructions[0].pos - 5 - call_pad;
        if (text[call_pos] != 0xE8)
            continue;

        if (call_pos < 7)
            continue;
        size_t lea_pos = call_pos - 7;
        static constexpr uint8_t lea_rcx[] = { 0x48, 0x8D, 0x0D };
        if (!bytes_equal(text, size, lea_pos, lea_rcx, sizeof(lea_rcx)))
            continue;

        size_t jump = 0;
        if (!conditional_jump_pos(text, size, lea_pos, jump) || jump < 3)
            continue;

        size_t test_pos = jump - 3;
        if (test_pos + 3 > size)
            continue;

        uint8_t test_rex = text[test_pos];
        uint8_t test_modrm = text[test_pos + 2];
        uint8_t test_rexb = test_rex & 1;
        uint8_t test_mod = test_modrm & 0xC0;
        uint8_t test_reg1 = test_modrm & 7;
        uint8_t test_reg2 = (test_modrm >> 3) & 7;

        constexpr uint8_t rex_w = 0x48;
        constexpr uint8_t rex_w_mask = 0xF8;
        if ((test_rex & rex_w_mask) != rex_w || test_mod != 0xC0 || test_reg1 != test_reg2)
            continue;

        for (size_t mov_pad = 0; mov_pad <= 3; ++mov_pad)
        {
            if (test_pos < 7 + mov_pad)
                continue;
            size_t mov_pos = test_pos - 7 - mov_pad;
            if (mov_pos + 7 > size)
                continue;

            uint8_t mov_rex = text[mov_pos];
            uint8_t mov_modrm = text[mov_pos + 2];
            uint8_t mov_rexw = (mov_rex >> 2) & 1;
            uint8_t mov_mod = mov_modrm & 0xC0;
            uint8_t mov_mem = mov_modrm & 7;
            uint8_t mov_reg = (mov_modrm >> 3) & 7;

            if ((mov_rex & rex_w_mask) == rex_w && mov_mod == 0 && mov_mem == 5 &&
                mov_rexw == test_rexb && mov_reg == test_reg1)
            {
                candidate.static_disp = mov_pos + 3;
                candidate.reflection_disp = lea_pos + 3;
                candidate.get_name_disp = call_pos + 1;
                return true;
            }
        }
    }

    return false;
}

static bool rva_in_section(DWORD rva, const SectionInfo& section)
{
    return rva >= section.rva && rva < section.rva + section.size;
}

static bool rva_in_any(DWORD rva, const std::array<SectionInfo, 16>& sections, size_t count)
{
    for (size_t i = 0; i < count; ++i)
    {
        if (rva_in_section(rva, sections[i]))
            return true;
    }
    return false;
}

static bool safe_name_equals(GetSingletonName get_name, const uint8_t* reflection, const char* expected)
{
    __try
    {
        const char* name = get_name(reflection);
        return name != nullptr && std::strcmp(name, expected) == 0;
    }
    __except (EXCEPTION_EXECUTE_HANDLER)
    {
        return false;
    }
}

static uintptr_t find_csevent_flag_man_static()
{
    uint8_t* base = reinterpret_cast<uint8_t*>(GetModuleHandleW(nullptr));
    if (base == nullptr)
        return 0;

    auto dos = reinterpret_cast<IMAGE_DOS_HEADER*>(base);
    if (dos->e_magic != IMAGE_DOS_SIGNATURE)
        return 0;

    auto nt = reinterpret_cast<IMAGE_NT_HEADERS64*>(base + dos->e_lfanew);
    if (nt->Signature != IMAGE_NT_SIGNATURE)
        return 0;

    IMAGE_SECTION_HEADER* section = IMAGE_FIRST_SECTION(nt);
    std::array<SectionInfo, 16> text_sections = {};
    std::array<SectionInfo, 16> data_sections = {};
    size_t text_count = 0;
    size_t data_count = 0;

    for (WORD i = 0; i < nt->FileHeader.NumberOfSections; ++i)
    {
        char name[9] = {};
        std::memcpy(name, section[i].Name, 8);
        DWORD size = section[i].Misc.VirtualSize;
        if (size == 0)
            size = section[i].SizeOfRawData;

        if (std::strcmp(name, ".text") == 0 && text_count < text_sections.size())
            text_sections[text_count++] = { ".text", base + section[i].VirtualAddress, section[i].VirtualAddress, size };
        if (std::strcmp(name, ".data") == 0 && data_count < data_sections.size())
            data_sections[data_count++] = { ".data", base + section[i].VirtualAddress, section[i].VirtualAddress, size };
    }

    for (size_t t = 0; t < text_count; ++t)
    {
        const SectionInfo& text_section = text_sections[t];
        const uint8_t* text = text_section.data;
        size_t text_size = text_section.size;

        for (size_t pos = 0; pos + 5 < text_size; ++pos)
        {
            if (text[pos] != 0xBA)
                continue;

            Fd4Candidate candidate = {};
            if (!try_fd4_candidate(text, text_size, pos, candidate))
                continue;

            DWORD static_rva = text_section.rva + static_cast<DWORD>(candidate.static_disp + 4) +
                static_cast<DWORD>(read_i32_unaligned(text + candidate.static_disp));
            DWORD reflection_rva = text_section.rva + static_cast<DWORD>(candidate.reflection_disp + 4) +
                static_cast<DWORD>(read_i32_unaligned(text + candidate.reflection_disp));
            DWORD get_name_rva = text_section.rva + static_cast<DWORD>(candidate.get_name_disp + 4) +
                static_cast<DWORD>(read_i32_unaligned(text + candidate.get_name_disp));

            if (!rva_in_any(static_rva, data_sections, data_count) ||
                !rva_in_any(get_name_rva, text_sections, text_count))
            {
                continue;
            }

            auto get_name = reinterpret_cast<GetSingletonName>(base + get_name_rva);
            auto reflection = base + reflection_rva;
            if (!safe_name_equals(get_name, reflection, "CSEventFlagMan"))
                continue;

            return reinterpret_cast<uintptr_t>(base + static_rva);
        }
    }

    return 0;
}

static bool find_flag_block_descriptor(uintptr_t map_address, uint32_t group, uint32_t& mode, uintptr_t& location)
{
    uintptr_t head = 0;
    if (!safe_read(reinterpret_cast<uintptr_t*>(map_address + 8), head) || head == 0)
        return false;

    uintptr_t root = 0;
    if (!safe_read(reinterpret_cast<uintptr_t*>(head + 8), root) || root == 0)
        return false;

    uintptr_t candidate = head;
    uintptr_t current = root;

    for (int depth = 0; current != 0 && depth < 128; ++depth)
    {
        bool is_nil = true;
        if (!safe_read(reinterpret_cast<bool*>(current + 25), is_nil))
            return false;
        if (is_nil)
            break;

        uint32_t key = 0;
        if (!safe_read(reinterpret_cast<uint32_t*>(current + 32), key))
            return false;

        if (!(key < group))
        {
            candidate = current;
            if (!safe_read(reinterpret_cast<uintptr_t*>(current), current))
                return false;
        }
        else
        {
            if (!safe_read(reinterpret_cast<uintptr_t*>(current + 16), current))
                return false;
        }
    }

    if (candidate == head)
        return false;

    uint32_t key = 0;
    if (!safe_read(reinterpret_cast<uint32_t*>(candidate + 32), key) || group < key)
        return false;

    if (!safe_read(reinterpret_cast<uint32_t*>(candidate + 40), mode))
        return false;
    if (!safe_read(reinterpret_cast<uintptr_t*>(candidate + 48), location))
        return false;

    return true;
}

static bool read_event_flag(uintptr_t event_flag_man, int flag_id, bool& value)
{
    if (event_flag_man == 0 || flag_id <= 0)
        return false;

    uint32_t flag = static_cast<uint32_t>(flag_id);
    uint32_t group = flag / 1000;
    uint32_t byte_index = (flag % 1000) / 8;
    uint32_t bit = 7 - ((flag % 1000) % 8);

    if (byte_index >= 125)
        return false;

    uintptr_t flag_blocks = 0;
    if (!safe_read(reinterpret_cast<uintptr_t*>(event_flag_man + 0x28), flag_blocks) || flag_blocks == 0)
        return false;

    uint32_t mode = 0;
    uintptr_t location = 0;
    if (!find_flag_block_descriptor(event_flag_man + 0x30, group, mode, location))
        return false;

    uintptr_t block = 0;
    if (mode == 1)
        block = flag_blocks + location * 125;
    else if (mode == 2)
        block = location;
    else
        return false;

    uint8_t byte_value = 0;
    if (!safe_read(reinterpret_cast<uint8_t*>(block + byte_index), byte_value))
        return false;

    value = (byte_value & (1u << bit)) != 0;
    return true;
}

static FlagBridgeShared* create_bridge_mapping(HANDLE& mapping)
{
    mapping = CreateFileMappingW(
        INVALID_HANDLE_VALUE,
        nullptr,
        PAGE_READWRITE,
        0,
        sizeof(FlagBridgeShared),
        kBridgeMappingName);

    if (mapping == nullptr)
        return nullptr;

    DWORD create_error = GetLastError();
    auto shared = static_cast<FlagBridgeShared*>(MapViewOfFile(
        mapping,
        FILE_MAP_ALL_ACCESS,
        0,
        0,
        sizeof(FlagBridgeShared)));

    if (shared == nullptr)
    {
        CloseHandle(mapping);
        mapping = nullptr;
        return nullptr;
    }

    if (create_error != ERROR_ALREADY_EXISTS || shared->magic != kBridgeMagic)
    {
        ZeroMemory(shared, sizeof(FlagBridgeShared));
        shared->magic = kBridgeMagic;
        shared->version = kBridgeVersion;
        shared->slot_count = kBridgeSlotCount;
    }

    return shared;
}

static void run_flag_bridge()
{
    HANDLE mapping = nullptr;
    FlagBridgeShared* shared = create_bridge_mapping(mapping);
    if (shared == nullptr)
    {
        append_log(L"Flag bridge shared memory creation failed.");
        return;
    }

    append_log(L"Flag bridge shared memory ready.");

    uintptr_t event_flag_static = 0;
    uintptr_t event_flag_man = 0;
    bool logged_resolved = false;

    while (true)
    {
        if (event_flag_static == 0)
            event_flag_static = find_csevent_flag_man_static();

        if (event_flag_static != 0)
            safe_read(reinterpret_cast<uintptr_t*>(event_flag_static), event_flag_man);

        shared->event_flag_man = static_cast<unsigned long long>(event_flag_man);
        shared->status = event_flag_man != 0 ? 2 : (event_flag_static != 0 ? 1 : 3);

        if (event_flag_man != 0 && !logged_resolved)
        {
            append_log(L"CSEventFlagMan resolved for flag bridge.");
            logged_resolved = true;
        }

        for (DWORD i = 0; i < kBridgeSlotCount; ++i)
        {
            LONG flag_id = shared->slots[i].flag_id;
            if (flag_id <= 0)
            {
                shared->slots[i].state = 0;
                continue;
            }

            bool active = false;
            if (event_flag_man != 0 && read_event_flag(event_flag_man, flag_id, active))
            {
                shared->slots[i].state = active ? 2 : 1;
                shared->slots[i].last_error = 0;
            }
            else
            {
                shared->slots[i].state = -1;
                shared->slots[i].last_error = 1;
            }

            InterlockedIncrement(&shared->slots[i].sequence);
        }

        Sleep(100);
    }
}

static DWORD WINAPI loader_thread(LPVOID)
{
    append_log(L"CMI Nightreign loader started external host mode.");

    wchar_t dir[MAX_PATH] = {};
    loader_directory(dir, MAX_PATH);

    wchar_t host_path[MAX_PATH] = {};
    swprintf_s(host_path, L"%s\\CMI.Host.exe", dir);

    DWORD attributes = GetFileAttributesW(host_path);
    if (attributes == INVALID_FILE_ATTRIBUTES || (attributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
    {
        append_log(L"CMI.Host.exe not found next to loader.");
        return 1;
    }

    wchar_t command_line[MAX_PATH + 8] = {};
    swprintf_s(command_line, L"\"%s\"", host_path);

    STARTUPINFOW startup = {};
    startup.cb = sizeof(startup);
    PROCESS_INFORMATION process = {};

    BOOL ok = CreateProcessW(
        host_path,
        command_line,
        nullptr,
        nullptr,
        FALSE,
        CREATE_NEW_PROCESS_GROUP,
        nullptr,
        dir,
        &startup,
        &process);

    if (!ok)
    {
        wchar_t buffer[256] = {};
        swprintf_s(buffer, L"CreateProcessW failed: %lu", GetLastError());
        append_log(buffer);
        return 1;
    }

    CloseHandle(process.hThread);
    CloseHandle(process.hProcess);
    append_log(L"CMI.Host.exe launched.");

    if (current_process_is_supported_for_flag_bridge())
    {
        run_flag_bridge();
    }
    else
    {
        append_log(L"Flag bridge skipped because this process is not nightreign.exe or eldenring.exe.");
    }

    return 0;
}

BOOL APIENTRY DllMain(HMODULE module, DWORD reason, LPVOID)
{
    if (reason == DLL_PROCESS_ATTACH)
    {
        g_module = module;
        DisableThreadLibraryCalls(module);

        HANDLE thread = CreateThread(nullptr, 0, loader_thread, nullptr, 0, nullptr);
        if (thread != nullptr)
        {
            CloseHandle(thread);
        }
    }

    return TRUE;
}
