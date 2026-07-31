const shell = document.querySelector("#sir-mockup");
const workscreen = document.querySelector("#tactical-workscreen");
const identityStatus = document.querySelector("#identity-status");
const conceptTitle = document.querySelector("#concept-title");
const conceptSummary = document.querySelector("#concept-summary");
const conceptLetter = document.querySelector(".concept-letter");
const modeReadout = document.querySelector("#mode-readout");
const modeDetail = document.querySelector("#mode-detail");

const concepts = {
  balanced: {
    letter: "A",
    title: "Balanced command desk",
    summary: "A compact default with both sidebars visible and a shallow timeline.",
  },
  focus: {
    letter: "B",
    title: "Field-focus workspace",
    summary: "Narrow supporting panels and a reduced timeline give the battlefield maximum area.",
  },
  analysis: {
    letter: "C",
    title: "Operations analysis desk",
    summary: "Wider inspectors and a deeper timeline prioritize planning and replay analysis.",
  },
};

const modalities = {
  editor: {
    label: "EDITOR",
    detail: "Authored map · revision 42",
  },
  plan: {
    label: "PLAN",
    detail: "Authored + predicted · accepted revision 17",
  },
  simulate: {
    label: "SIMULATE",
    detail: "Disposable runtime · tick 248",
  },
  review: {
    label: "REVIEW",
    detail: "Committed history · verified perspective",
  },
};

let transitions = 0;

function assertWorkscreenIdentity() {
  const current = document.querySelector("#tactical-workscreen");
  if (current !== workscreen) {
    identityStatus.textContent = "FAILED · workscreen replaced";
    identityStatus.dataset.failed = "true";
    throw new Error("The tactical workscreen was replaced.");
  }

  identityStatus.textContent = `retained · ${transitions} transition${transitions === 1 ? "" : "s"}`;
}

document.querySelectorAll("[data-concept]").forEach((button) => {
  button.addEventListener("click", () => {
    const concept = button.dataset.concept;
    document.querySelectorAll("[data-concept]").forEach((candidate) => {
      candidate.setAttribute("aria-pressed", String(candidate === button));
    });

    shell.classList.remove("concept-balanced", "concept-focus", "concept-analysis");
    shell.classList.add(`concept-${concept}`);
    conceptLetter.textContent = concepts[concept].letter;
    conceptTitle.textContent = concepts[concept].title;
    conceptSummary.textContent = concepts[concept].summary;
    transitions += 1;
    assertWorkscreenIdentity();
  });
});

document.querySelectorAll("[data-modality]").forEach((button) => {
  button.addEventListener("click", () => {
    const modality = button.dataset.modality;
    document.querySelectorAll("[data-modality]").forEach((candidate) => {
      candidate.setAttribute("aria-pressed", String(candidate === button));
    });

    shell.classList.remove(
      "modality-editor",
      "modality-plan",
      "modality-simulate",
      "modality-review",
    );
    shell.classList.add(`modality-${modality}`);
    modeReadout.textContent = modalities[modality].label;
    modeDetail.textContent = modalities[modality].detail;
    transitions += 1;
    assertWorkscreenIdentity();
  });
});

document.querySelector("#toggle-left").addEventListener("click", () => {
  shell.classList.toggle("left-hidden");
  transitions += 1;
  assertWorkscreenIdentity();
});

document.querySelector("#toggle-right").addEventListener("click", () => {
  shell.classList.toggle("right-hidden");
  transitions += 1;
  assertWorkscreenIdentity();
});

document.querySelector("#toggle-timeline").addEventListener("click", () => {
  shell.classList.toggle("timeline-hidden");
  transitions += 1;
  assertWorkscreenIdentity();
});

document.querySelector(".timeline-collapse").addEventListener("click", () => {
  shell.classList.toggle("timeline-hidden");
  transitions += 1;
  assertWorkscreenIdentity();
});

document.querySelectorAll("[data-hide]").forEach((button) => {
  button.addEventListener("click", () => {
    shell.classList.add(`${button.dataset.hide}-hidden`);
    transitions += 1;
    assertWorkscreenIdentity();
  });
});

document.querySelectorAll("[data-move]").forEach((button) => {
  button.addEventListener("click", () => {
    shell.classList.toggle("sidebars-swapped");
    transitions += 1;
    assertWorkscreenIdentity();
  });
});

window.addEventListener("keydown", (event) => {
  if (event.target instanceof HTMLInputElement) return;
  const shortcuts = { "1": "editor", "2": "plan", "3": "simulate", "4": "review" };
  const modality = shortcuts[event.key];
  if (!modality) return;
  document.querySelector(`[data-modality="${modality}"]`).click();
});

assertWorkscreenIdentity();
