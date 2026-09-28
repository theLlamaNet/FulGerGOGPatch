#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <stdio.h>
#include <string.h>

// Core API v1 plugin ABI reconstructed from gog_pop1.dll's virtual table.
// This plugin targets only the exact bundled POP2/POP3 widescreen-fix DLLs.
typedef void (__cdecl *NkRow)(void *, float, int);
typedef void (__cdecl *NkLabel)(void *, const char *, int);
typedef void (__cdecl *NkLabelf)(void *, int, const char *, ...);
typedef int (__cdecl *NkSlider)(void *, int, int *, int, int);

static int g_percent = 100;
static bool g_dirty = false;
static bool g_fix_load_attempted = false;
static const char *g_fix_dll = 0;
static float *g_multiplier = 0;
static BYTE *g_fix_base = 0;
static const char *g_game_ini = 0;
static NkRow g_row = 0;
static NkLabel g_label = 0;
static NkLabelf g_labelf = 0;
static NkSlider g_slider = 0;

static void attach_multiplier()
{
    HMODULE mod = GetModuleHandleA("pop2w.dll");
    if (mod) g_game_ini = "pop2.ini";
    else {
        mod = GetModuleHandleA("pop3w.dll");
        if (mod) g_game_ini = "pop3.ini";
    }
    if (mod) {
        g_fix_base = (BYTE *)mod;
        g_multiplier = (float *)(g_fix_base + 0x191E8);
    }
}

static void ensure_fix_loaded()
{
    if (g_multiplier || g_fix_load_attempted) return;
    g_fix_load_attempted = true;
    char path[MAX_PATH];
    if (!GetModuleFileNameA(0, path, MAX_PATH)) return;
    const char *exe = strrchr(path, '\\');
    exe = exe ? exe + 1 : path;
    if (_stricmp(exe, "pop2_gogfix.exe") == 0) g_fix_dll = "pop2w.dll";
    if (_stricmp(exe, "pop3_gogfix.exe") == 0) g_fix_dll = "pop3w.dll";
    if (!g_fix_dll) return;
    HMODULE loaded = LoadLibraryA(g_fix_dll);
    if (!loaded) {
        char message[160];
        sprintf_s(message, sizeof(message), "FulGer FOV: unable to load %s (Win32 %lu)\n", g_fix_dll, GetLastError());
        OutputDebugStringA(message);
        char *slash = strrchr(path, '\\');
        if (slash && strcpy_s(slash + 1, MAX_PATH - (slash + 1 - path), "fulger_fov.log") == 0) {
            FILE *log = 0;
            if (fopen_s(&log, path, "ab") == 0 && log) { fputs(message, log); fclose(log); }
        }
    }
    attach_multiplier();
}

static void apply_fov()
{
    if (!g_fix_base) return;
    const float scale = g_percent / 100.f;
    const float aspect_factor = *(float *)(g_fix_base + 0x1A18C);
    if (aspect_factor <= 0.f || aspect_factor > 10.f) return;
    *g_multiplier = scale;
    *(float *)(g_fix_base + 0x1A178) = aspect_factor * scale;
    *(float *)(g_fix_base + 0x1A1A4) = 1.f / scale;
    *(float *)(g_fix_base + 0x1A1A0) = 2.f / scale;
    *(float *)(g_fix_base + 0x1A190) = 2.f / scale;
    *(float *)(g_fix_base + 0x1A170) = 2.f / scale;
    *(float *)(g_fix_base + 0x1A19C) = 2.f / scale;
}

static void save_setting(const char *file, const char *key, const char *value)
{
    char path[MAX_PATH], tmp[MAX_PATH], line[1024];
    GetModuleFileNameA(0, path, MAX_PATH);
    char *slash = strrchr(path, '\\');
    if (!slash) return;
    if (strcpy_s(slash + 1, MAX_PATH - (slash + 1 - path), file)) return;
    if (strcpy_s(tmp, MAX_PATH, path) || strcat_s(tmp, MAX_PATH, ".tmp")) return;
    FILE *in = 0;
    if (fopen_s(&in, path, "rb")) return;
    if (!in) return;
    FILE *out = 0;
    if (fopen_s(&out, tmp, "wb")) { fclose(in); return; }
    if (!out) { fclose(in); return; }
    bool found = false;
    while (fgets(line, sizeof(line), in)) {
        char *s = line;
        while (*s == ' ' || *s == '\t') ++s;
        if (_strnicmp(s, key, strlen(key)) == 0) {
            char *p = s + strlen(key);
            while (*p == ' ' || *p == '\t') ++p;
            if (*p == '=') { fprintf(out, "%s=%s\r\n", key, value); found = true; continue; }
        }
        fputs(line, out);
    }
    if (!found) fprintf(out, "%s=%s\r\n", key, value);
    fclose(out);
    fclose(in);
    MoveFileExA(tmp, path, MOVEFILE_REPLACE_EXISTING);
}

struct FulgerFovPlugin {
    virtual const char *name() { return "FulGer Patch FOV"; }
    virtual const char *description() { return "Camera FOV for Warrior Within and The Two Thrones"; }
    virtual const char *category() { return "Camera"; }
    virtual void onOptionsLoaded(void *) {
        attach_multiplier();
        if (g_multiplier) {
            int value = (int)(*g_multiplier * 100.f + .5f);
            if (value >= 50 && value <= 200) g_percent = value;
        }
    }
    virtual void onPostCoreInit() { attach_multiplier(); }
    virtual void onResolutionChange(unsigned, unsigned) {}
    virtual void onGameFrame(unsigned, unsigned) {
        ensure_fix_loaded();
        if (!g_multiplier) attach_multiplier();
        if (g_dirty && g_multiplier) { apply_fov(); g_dirty = false; }
    }
    virtual void onOptionsDraw(void *ctx) {
        if (!g_row || !g_label || !g_labelf || !g_slider) {
            HMODULE core = GetModuleHandleA("gog_core.dll");
            if (!core) return;
            g_row = (NkRow)GetProcAddress(core, "nk_layout_row_dynamic");
            g_label = (NkLabel)GetProcAddress(core, "nk_label");
            g_labelf = (NkLabelf)GetProcAddress(core, "nk_labelf");
            g_slider = (NkSlider)GetProcAddress(core, "nk_slider_int");
            if (!g_row || !g_label || !g_labelf || !g_slider) return;
        }
        if (!g_multiplier) attach_multiplier();
        if (!g_multiplier) ensure_fix_loaded();
        // Match gog_pop1.dll: one dynamic three-column row, a label,
        // the slider, and a formatted value. Zero requests the theme height.
        g_row(ctx, 0.f, 3);
        g_label(ctx, "FOV multiplier %", 1);
        if (g_slider(ctx, 50, &g_percent, 200, 1)) {
            g_dirty = true;
            if (g_multiplier) { apply_fov(); g_dirty = false; }
        }
        g_labelf(ctx, 0, "%d%%", g_percent);
    }
    virtual void onOptionsSave(void *) {
        char value[32];
        sprintf_s(value, sizeof(value), "%.3f", g_percent == 100 ? 1.001f : g_percent / 100.f);
        if (g_game_ini) save_setting(g_game_ini, "fov_multiplier", value);
    }
    virtual void onPreCoreShutdown() {}
};

static FulgerFovPlugin g_plugin;
extern "C" __declspec(dllexport) const char *pluginName() { return "FulGer Patch FOV"; }
extern "C" __declspec(dllexport) bool pluginCoreVersionCompatible(unsigned version) { return version == 1; }
extern "C" __declspec(dllexport) bool pluginGameVersionCompatible(unsigned crc) {
    return crc == 0x77DA8770u || crc == 0xA2538C99u;
}
extern "C" __declspec(dllexport) void *pluginGet() { ensure_fix_loaded(); return &g_plugin; }
