export function openDeliverySupport(host) {
  if (!host || host.querySelector("[data-delivery-support-loaded]")) return;

  const panel = document.createElement("p");
  panel.dataset.deliverySupportLoaded = "true";
  panel.textContent =
    "This support panel loads on demand; the simulator remains available before it is fetched.";
  host.append(panel);
}
