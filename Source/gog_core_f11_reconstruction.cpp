// Semi-decompiled F11 menu reconstruction for the bundled 32-bit gog_core.dll.
// This is explanatory source, not a replacement build of the proprietary DLL.
// Only patch-gog-core-title.py changes the shipped DLL, in one verified string
// slot. Addresses below are for the unrebased image (preferred base 0x10000000).

// Verified in the bundled image:
//   0x100015BB pushes 0x10043AA4, the title passed to nk_begin.
//   0x10043AA4 holds the 24-byte title slot; 0x10043ABC starts "Limit FPS".
//   0x10043AD4 says "plugin %s -> onOptionsDraw".
//   0x10043AF0 says "Save Options".
//   0x10043B24 says "plugin %s -> onOptionsSave".
//   nk_begin, nk_layout_row_dynamic, nk_labelf, and nk_slider_int are exported
//   from gog_core.dll. The plugin calls these exports through GetProcAddress.

#if 0  // Deliberately non-compilable pseudocode; opaque internals are unknown.
void draw_f11_options(GogCore *core, nk_context *ctx)
{
    nk_rect bounds = compute_options_window_bounds(core);
    if (nk_begin(ctx, "FulGer Patch Options", bounds, 0x41)) {
        draw_limit_fps_control(core, ctx);
        draw_max_fps_control(core, ctx);

        // The core discovers compatible gog_*.dll plugins and invokes their
        // virtual onOptionsDraw callback in the same Nuklear context.
        for (Plugin *plugin : core->compatible_plugins)
            plugin->onOptionsDraw(ctx);

        if (nk_button_label(ctx, "Save Options")) {
            save_core_options_to_gog_toml(core);
            for (Plugin *plugin : core->compatible_plugins)
                plugin->onOptionsSave(ctx);
        }
    }
    nk_end(ctx);
}
#endif

// The core menu itself does not implement the POP2/POP3 FOV slider.
// Source/gog_fulger_fov.cpp implements the separate plugin and uses the same
// three-column label/slider/value layout seen in gog_pop1.dll's F11 callback.
