const opener = document.querySelector("#open-delivery-support");

opener?.addEventListener("click", async () => {
  const support = await import("./deferred-delivery-support.js");
  support.openDeliverySupport(document.querySelector("#sir-delivery-support"));
});
