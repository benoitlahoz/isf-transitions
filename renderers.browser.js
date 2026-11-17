// Runs in the browser (bundled by renderers.test.mjs): plays a transition between a red
// and a blue image with each renderer, and reads what is drawn.

import { Renderer } from 'interactive-shader-format';
import { IsfRenderer } from '@isf-studio/isf-renderer';

const SIZE = 32;
const RED = [255, 0, 0];
const BLUE = [0, 0, 255];
// Not exactly 0 and 1, where some transitions divide by zero.
const START = 0.001;
const END = 0.999;
const GRAY = [128, 128, 128]; // for the extra textures (a luma, a displacement map...)

const solid = (color) => {
  const canvas = document.createElement('canvas');
  canvas.width = canvas.height = SIZE;
  const context = canvas.getContext('2d');
  context.fillStyle = `rgb(${color})`;
  context.fillRect(0, 0, SIZE, SIZE);
  return canvas;
};

const nextFrame = () => new Promise((resolve) => requestAnimationFrame(resolve));

// The mean red, green and blue of RGBA pixels.
const mean = (data) => {
  const sum = [0, 0, 0];
  for (let i = 0; i < data.length; i += 4) {
    for (let c = 0; c < 3; c++) sum[c] += data[i + c];
  }
  return sum.map((value) => Math.round(value / (data.length / 4)));
};

// The frame shows the expected image: the one of the transition that is not its opposite.
const shows = (frame, expected) => (expected === RED ? frame[0] > frame[2] : frame[2] > frame[0]);

// interactive-shader-format-js draws on the context it is given: the pixel is read right after.
const withInteractiveShaderFormat = (gl, canvas, source) => {
  const renderer = new Renderer(gl);
  renderer.loadSource(source);
  if (!renderer.valid) throw new Error(`${renderer.error}`);

  const images = { startImage: solid(RED), endImage: solid(BLUE) };
  for (const name of Object.keys(renderer.uniforms)) {
    if (renderer.uniforms[name].type === 't' && !(name in images)) {
      images[name] = solid(GRAY);
    }
  }

  const frames = {};
  const buffer = new Uint8Array(SIZE * SIZE * 4);
  for (const progress of [START, END]) {
    // As in its own examples, the images are given again before each frame
    // (the images given before the very first frame are drawn black).
    for (let frame = 0; frame < 2; frame++) {
      for (const [name, image] of Object.entries(images)) renderer.setValue(name, image);
      renderer.setValue('progress', progress);
      renderer.draw(canvas);
    }
    gl.readPixels(0, 0, SIZE, SIZE, gl.RGBA, gl.UNSIGNED_BYTE, buffer);
    frames[progress === START ? 'start' : 'end'] = mean(buffer);
  }
  return frames;
};

// isf-renderer draws on its own: the canvas is read when it has drawn a frame.
const withIsfRenderer = async (source) => {
  // A canvas for each shader: a context keeps what the previous shader left in it.
  const canvas = document.createElement('canvas');
  canvas.width = canvas.height = SIZE;
  const renderer = new IsfRenderer();
  const errors = [];
  renderer.onError = (error) => errors.push(String(error));

  try {
    renderer.bind(canvas);
    renderer.setSize(SIZE, SIZE);
    await renderer.load(source);
    renderer.set('startImage', solid(RED).toDataURL());
    renderer.set('endImage', solid(BLUE).toDataURL());
    for (const input of renderer.inputs) {
      if (input.TYPE === 'image' && !/^(start|end)Image$/.test(input.NAME)) {
        renderer.set(input.NAME, solid(GRAY).toDataURL());
      }
    }

    const copy = document.createElement('canvas');
    copy.width = copy.height = SIZE;
    const context = copy.getContext('2d', { willReadFrequently: true });

    const frames = {};
    for (const [progress, expected] of [[START, RED], [END, BLUE]]) {
      renderer.set('progress', progress);

      // The images are decoded asynchronously: wait for the frame to show the expected image.
      for (let frame = 0; frame < 120; frame++) {
        await nextFrame();
        context.clearRect(0, 0, SIZE, SIZE);
        context.drawImage(canvas, 0, 0, SIZE, SIZE);
        const name = progress === START ? 'start' : 'end';
        frames[name] = mean(context.getImageData(0, 0, SIZE, SIZE).data);
        if (shows(frames[name], expected)) break;
      }
    }
    return { frames, errors };
  } finally {
    renderer.dispose();
    // A page keeps a limited number of contexts.
    canvas.getContext('webgl2')?.getExtension('WEBGL_lose_context')?.loseContext();
  }
};

window.playTransitions = async (sources) => {
  const results = {};

  const isfJsCanvas = document.createElement('canvas');
  isfJsCanvas.width = isfJsCanvas.height = SIZE;
  const gl = isfJsCanvas.getContext('webgl2', { preserveDrawingBuffer: true });

  for (const [name, source] of Object.entries(sources)) {
    const result = {};
    try {
      result.interactiveShaderFormat = withInteractiveShaderFormat(gl, isfJsCanvas, source);
    } catch (error) {
      result.interactiveShaderFormat = { error: String(error.message ?? error) };
    }
    try {
      result.isfRenderer = await withIsfRenderer(source);
    } catch (error) {
      result.isfRenderer = { error: String(error.message ?? error) };
    }
    results[name] = result;
  }

  return { webgl: !!gl, results };
};
