// Minimal drag-to-orbit + wheel-to-zoom camera control.
// Self-contained (no ES module imports), works with the UMD three.min.js build.
// Usage: const controls = createOrbitControls(camera, rendererDomElement, { target: new THREE.Vector3(0,0,0) });
//        call controls.update() once per animation frame before rendering.
function createOrbitControls(camera, domElement, opts) {
  const target = (opts && opts.target) || new THREE.Vector3(0, 0, 0);
  let radius = camera.position.clone().sub(target).length();
  let theta = Math.atan2(camera.position.x - target.x, camera.position.z - target.z);
  let phi = Math.acos(Math.min(1, Math.max(-1, (camera.position.y - target.y) / radius)));

  let dragging = false;
  let lastX = 0, lastY = 0;

  function apply() {
    phi = Math.min(Math.max(phi, 0.05), Math.PI - 0.05);
    const sinPhi = Math.sin(phi);
    camera.position.x = target.x + radius * sinPhi * Math.sin(theta);
    camera.position.y = target.y + radius * Math.cos(phi);
    camera.position.z = target.z + radius * sinPhi * Math.cos(theta);
    camera.lookAt(target);
  }

  domElement.addEventListener('pointerdown', (e) => {
    dragging = true; lastX = e.clientX; lastY = e.clientY;
    domElement.setPointerCapture(e.pointerId);
  });
  domElement.addEventListener('pointerup', () => { dragging = false; });
  domElement.addEventListener('pointerleave', () => { dragging = false; });
  domElement.addEventListener('pointermove', (e) => {
    if (!dragging) return;
    const dx = e.clientX - lastX, dy = e.clientY - lastY;
    lastX = e.clientX; lastY = e.clientY;
    theta -= dx * 0.006;
    phi -= dy * 0.006;
    apply();
  });
  domElement.addEventListener('wheel', (e) => {
    e.preventDefault();
    radius *= (1 + e.deltaY * 0.001);
    radius = Math.min(Math.max(radius, 1.5), 40);
    apply();
  }, { passive: false });

  // touch support (single-finger orbit, pinch zoom)
  let pinchDist = 0;
  domElement.addEventListener('touchstart', (e) => {
    if (e.touches.length === 1) { dragging = true; lastX = e.touches[0].clientX; lastY = e.touches[0].clientY; }
    else if (e.touches.length === 2) {
      dragging = false;
      pinchDist = Math.hypot(e.touches[0].clientX - e.touches[1].clientX, e.touches[0].clientY - e.touches[1].clientY);
    }
  }, { passive: true });
  domElement.addEventListener('touchmove', (e) => {
    if (e.touches.length === 1 && dragging) {
      const dx = e.touches[0].clientX - lastX, dy = e.touches[0].clientY - lastY;
      lastX = e.touches[0].clientX; lastY = e.touches[0].clientY;
      theta -= dx * 0.006; phi -= dy * 0.006; apply();
    } else if (e.touches.length === 2) {
      const d = Math.hypot(e.touches[0].clientX - e.touches[1].clientX, e.touches[0].clientY - e.touches[1].clientY);
      radius *= (1 + (pinchDist - d) * 0.003);
      radius = Math.min(Math.max(radius, 1.5), 40);
      pinchDist = d; apply();
    }
  }, { passive: true });
  domElement.addEventListener('touchend', () => { dragging = false; });

  apply();
  return { update: apply };
}
