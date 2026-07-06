const BORROW_CART_KEY = "ebms_borrow_cart";

function getBorrowCart() {
  const raw = sessionStorage.getItem(BORROW_CART_KEY);
  if (!raw) return [];
  try {
    const items = JSON.parse(raw);
    return Array.isArray(items) ? items : [];
  } catch {
    return [];
  }
}

function saveBorrowCart(items) {
  sessionStorage.setItem(BORROW_CART_KEY, JSON.stringify(items));
  $(document).trigger("borrowCartUpdated");
}

function isInBorrowCart(equipmentId) {
  return getBorrowCart().some(function (item) {
    return item.id === equipmentId;
  });
}

function addToBorrowCart(equipment) {
  const cart = getBorrowCart();
  if (cart.some(function (item) {
    return item.id === equipment.id;
  })) {
    return;
  }
  cart.push({
    id: equipment.id,
    name: equipment.name,
    serialNumber: equipment.serialNumber,
    categoryName: equipment.categoryName || ""
  });
  saveBorrowCart(cart);
}

function removeFromBorrowCart(equipmentId) {
  const cart = getBorrowCart().filter(function (item) {
    return item.id !== equipmentId;
  });
  saveBorrowCart(cart);
}

function clearBorrowCart() {
  sessionStorage.removeItem(BORROW_CART_KEY);
  $(document).trigger("borrowCartUpdated");
}

function getBorrowCartCount() {
  return getBorrowCart().length;
}

function borrowCartToApiItems() {
  return getBorrowCart().map(function (item) {
    return { equipmentId: item.id, quantity: 1 };
  });
}
