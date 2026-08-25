# Example: Probe Light Lab

`probe-light-lab.template.html` is the reference build this skill produced
for a space probe — an interactive render with sliders for dish roughness,
metalness, hull iridescence, and film thickness, plus a toggleable
ray-bounce overlay. It's the source template *before* the vendor scripts are
inlined: the two script tags contain `/* __THREE_JS__ */` and
`/* __ORBIT_CONTROLS_JS__ */` placeholders.

To turn it into a runnable page, substitute those comments with the full
contents of `../assets/vendor/three.min.js` and `../assets/orbit-controls.js`
respectively (see `references/threejs-patterns.md` §1). The compiled output
isn't checked in here to avoid duplicating the ~650KB vendor bundle a second
time — regenerate it on demand, e.g.:

```bash
python3 - <<'PY'
tpl = open("probe-light-lab.template.html").read()
three = open("../assets/vendor/three.min.js").read()
orbit = open("../assets/orbit-controls.js").read()
tpl = tpl.replace("/* __THREE_JS__ */", three).replace("/* __ORBIT_CONTROLS_JS__ */", orbit)
open("probe-light-lab.compiled.html", "w").write(tpl)
PY
```

Use this file as a starting point for a new object: swap the probe assembly
section for your own primitives, keep the material/slider/ray-overlay
patterns.
