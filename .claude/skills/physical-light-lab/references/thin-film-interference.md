# Thin-film interference: why foil and oil slicks shift color

Roughness and Fresnel explain *how much* light reflects. Thin-film
interference explains why some surfaces reflect *different colors depending
on viewing angle* — the rainbow sheen on a soap bubble, an oil slick, an
oxidized titanium ring, a beetle's shell, and — the reason this matters for
a space probe — the gold/amber shimmer on multi-layer insulation (MLI)
blankets and Kapton foil, which are wrapped in a film only a few hundred
nanometers thick.

## The physics

When a transparent film (thickness on the order of the wavelength of light,
~100–1000 nanometers) sits on top of a reflective base, incoming light
splits into two reflections:

1. A reflection off the **top surface** of the film (air → film boundary).
2. A reflection off the **bottom surface** of the film (film → base
   boundary), which has to travel down through the film and back up first.

Reflection #2 travels a longer path than #1. That extra distance means the
two reflected waves are out of phase by an amount that depends on:

- the film's **thickness** (thicker film = longer extra path = different
  phase shift),
- the **viewing/incidence angle** (more oblique = longer path through the
  film, which is why the color visibly shifts as you rotate the object —
  this is the signature that distinguishes iridescence from ordinary
  pigment color, which doesn't shift with angle),
- and the light's **wavelength**, because phase shift is measured in
  fractions of a wavelength — so red, green, and blue light interfere
  differently at the *same* physical thickness and angle.

Where the two waves arrive in phase, that wavelength constructively
interferes and gets reflected strongly. Where they arrive out of phase, that
wavelength destructively cancels. Because this happens independently across
the visible spectrum, different thicknesses (or different angles on a
uniform film) reflect different dominant colors — that's the rainbow banding
you see on an oil slick, and it's why iridescent color is a *structural*
color, produced by wave interference, not by a pigment absorbing/reflecting
fixed wavelengths.

## Mapping to three.js `MeshPhysicalMaterial`

three.js implements this directly (it's part of the physically-based
material, not a hack) via three properties:

| Property | What it controls |
|---|---|
| `iridescence` | Blend strength of the thin-film effect, 0 (off) – 1 (full) |
| `iridescenceIOR` | Index of refraction of the film (~1.3 for a soap-film-like coating, ~1.5–2.0 for denser coatings) |
| `iridescenceThicknessRange` | `[minNm, maxNm]` — three.js internally varies the *effective* thickness per-pixel based on the normal/view angle within this range, which is what produces the color-band sweep across a curved surface |

**Building intuition for a user:** expose `iridescenceThicknessRange` as a
slider over the max value (e.g. 100–1000 nm) and `iridescence` as a 0–1
strength slider. Rotating the object (via orbit controls) while iridescence
is active is the single most convincing demo of "this is angle-dependent,
not a paint color" — call that out explicitly in the UI copy, and consider
defaulting the camera to auto-rotate slowly so the shift is visible even
before the user touches anything.

Use this on the parts of the probe that are physically foil-wrapped
(the body's MLI blanket, tanks) — not on solar panels or the antenna dish,
which are governed by ordinary microfacet reflectance instead. Mixing the
two correctly on the same object is itself a good teaching moment: not every
shiny surface is iridescent, and not every metal has a thin film.
