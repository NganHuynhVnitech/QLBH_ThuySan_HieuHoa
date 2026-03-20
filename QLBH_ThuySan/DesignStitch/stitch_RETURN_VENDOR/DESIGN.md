# Design System Document

## 1. Overview & Creative North Star: "The Industrial Curator"
This design system moves away from the utilitarian, spreadsheet-heavy aesthetics common in logistics software. Instead, it adopts the persona of **"The Industrial Curator."** It treats inventory data not as mere entries, but as a prestigious archive. 

The Creative North Star focuses on **High-Contrast Editorial Clarity**. We achieve this by utilizing an expansive typographic scale (using Manrope for structural headers) and replacing rigid, boxy grids with **Tonal Layering**. By using intentional asymmetry and varying surface heights, the system feels breathable, premium, and authoritative, ensuring that even high-density inventory lists feel organized rather than overwhelming.

---

## 2. Colors
Our palette is rooted in a professional, high-stability foundation. It utilizes deep forest greens for growth and confirmation, and clinical reds for critical destructive actions.

- **Primary (`#00450d`):** The "Forest Green." Used exclusively for confirmation and success. To add "soul," use a subtle gradient from `primary` to `primary_container` on large CTAs.
- **Secondary (`#466270`):** A sophisticated slate-grey used for neutral actions like "Cancel" or "Draft," providing a calmer alternative to stark black.
- **Tertiary (`#7c000b`):** The "Action Red." Reserved strictly for deletions and critical errors.
- **Surface Hierarchy:** 
    - **Background (`#f9f9f9`):** The canvas.
    - **Surface Container Lowest (`#ffffff`):** Reserved for the most important interactive elements (e.g., the actual input fields or active cards).
    - **Surface Container High/Highest (`#e8e8e8`/`#e2e2e2`):** Used for structural backgrounds to create depth.

**The "No-Line" Rule:** We prohibit the use of 1px solid borders for sectioning large areas of the UI. Separation must be achieved through background shifts (e.g., a `surface-container-low` header bar sitting on a `surface` body). 

**The "Glass & Gradient" Rule:** For floating modals or "add item" drawers, use a backdrop-blur (12px–20px) combined with a semi-transparent `surface_container_lowest` to create a "frosted glass" effect that keeps the user grounded in the inventory context.

---

## 3. Typography
The system employs a dual-typeface strategy to balance editorial elegance with functional legibility.

- **Structural Headers (Manrope):** Use `display` and `headline` scales for page titles and section headers. Manrope’s geometric nature provides a modern, architectural feel.
- **Functional Data (Inter):** Use `title`, `body`, and `label` scales for all data entry, table cells, and button text. Inter is chosen for its exceptional readability at small sizes (`body-sm`: 0.75rem).

**Hierarchy Strategy:** 
- **Page Titles:** Use `headline-lg` in `on_surface`.
- **Field Labels:** Use `label-md` or `label-sm` with `on_surface_variant` to keep the UI clean while maintaining accessibility.

---

## 4. Elevation & Depth
In this design system, depth is a functional tool, not a decoration. We move away from traditional drop shadows in favor of **Tonal Layering**.

- **The Layering Principle:** To create a "lifted" effect, place a `surface_container_lowest` (#ffffff) card on a `surface_container` (#eeeeee) background. The contrast in light alone defines the boundary.
- **Ambient Shadows:** For floating elements like dropdown menus or tooltips, use highly diffused shadows: `0px 8px 24px rgba(26, 28, 28, 0.06)`. This mimics soft, natural gallery lighting.
- **The "Ghost Border" Fallback:** If a container requires a border for accessibility (e.g., an input field), use the `outline_variant` token at **20% opacity**. Never use 100% opaque grey borders for layout containment.
- **Glassmorphism:** Use `surface_variant` with 80% opacity and a `blur(10px)` for secondary navigation or side-panels to maintain a sense of environmental awareness within the app.

---

## 5. Components

### Buttons
- **Primary (Confirm):** `primary` background, `on_primary` text. Use `roundedness-md` (0.375rem).
- **Secondary (Cancel/Neutral):** `secondary` background or `outline` ghost style.
- **Tertiary (Delete):** `tertiary` background with `on_tertiary` text. Only used for final destructive steps.
- **Padding:** Vertical `spacing-2.5`, Horizontal `spacing-6`.

### Input Fields & Selects
- **Surface:** Must be `surface_container_lowest` (#ffffff).
- **Border:** Ghost Border (20% `outline_variant`). 
- **Corners:** `roundedness-md`.
- **Focus State:** 2px solid `primary_fixed` to provide a clear, forest-green "active" glow.

### Cards & Inventory Lists
- **No Dividers:** Forbid the use of horizontal lines between rows. Use `spacing-4` of vertical white space or alternating tonal shifts (zebra striping using `surface` and `surface_container_low`).
- **Nesting:** Place a "Details" card (`surface_container_lowest`) inside a "Category" section (`surface_container_low`) to show relationship through indentation and tone rather than lines.

### Inventory Chips
- **Status Chips:** Use `primary_fixed` for "In Stock" and `tertiary_fixed` for "Out of Stock." Typography should be `label-sm` in all-caps with 0.05em letter spacing for an editorial look.

---

## 6. Do's and Don'ts

### Do
- **DO** use white space as a structural element. If a section feels cluttered, increase the spacing from `spacing-4` to `spacing-8`.
- **DO** use `display-sm` for large numerical data (e.g., total stock count) to give it visual "weight."
- **DO** align all form elements to a strict vertical axis to maintain the "Industrial Curator" precision.

### Don't
- **DON'T** use 1px solid `#cccccc` lines to separate table rows; it creates visual "noise" and looks dated.
- **DON'T** use standard "drop shadows" (e.g., black with 25% opacity). It muddies the clean, white aesthetic.
- **DON'T** use high-saturation greens or reds. Stick strictly to the "Forest" (`#00450d`) and "Deep Red" (`#7c000b`) tones provided in the palette to maintain the premium feel.
- **DON'T** crowd the edges. Ensure all containers have a minimum internal padding of `spacing-5`.