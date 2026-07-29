const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)");

const ensureTooltip = () => {
  let tooltip = document.querySelector(".sir-svg-tooltip");
  if (!tooltip) {
    tooltip = document.createElement("div");
    tooltip.className = "sir-svg-tooltip";
    tooltip.setAttribute("role", "tooltip");
    tooltip.hidden = true;
    document.body.append(tooltip);
  }
  return tooltip;
};

const positionTooltip = (tooltip, target) => {
  const box = target.getBoundingClientRect();
  const left = Math.min(
    window.innerWidth - tooltip.offsetWidth - 16,
    Math.max(16, box.left + box.width / 2 - tooltip.offsetWidth / 2),
  );
  const top = Math.max(16, box.top - tooltip.offsetHeight - 12);
  tooltip.style.left = `${left}px`;
  tooltip.style.top = `${top}px`;
};

const enhanceExplainer = (figure) => {
  const tooltip = ensureTooltip();
  const targets = figure.querySelectorAll("[data-svg-tip]");
  const hide = () => {
    tooltip.hidden = true;
    tooltip.textContent = "";
  };

  for (const target of targets) {
    const show = () => {
      tooltip.textContent = target.dataset.svgTip;
      tooltip.hidden = false;
      requestAnimationFrame(() => positionTooltip(tooltip, target));
    };
    target.addEventListener("mouseenter", show);
    target.addEventListener("mouseleave", hide);
    target.addEventListener("focus", show);
    target.addEventListener("blur", hide);
    target.addEventListener("keydown", (event) => {
      if (event.key === "Escape") {
        hide();
        target.blur();
      }
    });
  }

  const button = figure.querySelector(".sir-motion-toggle");
  const setPaused = (paused) => {
    figure.classList.toggle("is-paused", paused);
    if (button) {
      button.setAttribute("aria-pressed", String(paused));
      button.textContent = paused ? "Play motion" : "Pause motion";
    }
  };

  setPaused(reduceMotion.matches);
  button?.addEventListener("click", () => {
    setPaused(!figure.classList.contains("is-paused"));
  });
};

const simplifyBrand = () => {
  const brand = document.querySelector("header .start > a");
  const label = brand?.querySelector("strong");
  if (!brand || !label || brand.querySelector(".sir-brand-subtitle")) return;
  label.textContent = "S.I.R.";
  const subtitle = document.createElement("span");
  subtitle.className = "sir-brand-subtitle";
  subtitle.textContent = "Field Manual";
  brand.append(subtitle);
};

document.addEventListener("DOMContentLoaded", () => {
  simplifyBrand();
  document.querySelectorAll("[data-svg-explainer]").forEach(enhanceExplainer);
});
