// Shop / Ecommerce - JavaScript Module

$(document).ready(function () {
    // State variables
    let currentPage = 1;
    let pageSize = 12;
    let sortBy = 'Name';
    let sortOrder = 'asc';
    let searchTerm = '';
    let searchTimeout = null;
    let currentView = 'grid';
    let selectedCategoryId = '';

    // Initialize
    if (typeof window.targetCategoryId !== 'undefined' && window.targetCategoryId) {
        selectedCategoryId = window.targetCategoryId;
    }

    loadCategories();

    // Only load products if we are on the product list page
    if ($('#productsGrid').length > 0) {
        loadProducts();
    }

    attachEventHandlers();

    // ========================================
    // Event Handlers
    // ========================================
    function attachEventHandlers() {
        // Search functionality with debounce
        $('#productSearch').on('input', function () {
            const val = $(this).val();
            $('#clearSearch').toggle(val.length > 0);

            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function () {
                searchTerm = $('#productSearch').val();
                currentPage = 1;
                loadProducts();
            }, 300);
        });

        // Header search
        $('#headerSearchInput').on('keypress', function (e) {
            if (e.which === 13) {
                searchTerm = $(this).val();
                currentPage = 1;
                loadProducts();
                $('html, body').animate({
                    scrollTop: $('#products').offset().top - 100
                }, 500);
            }
        });

        // Clear search
        $('#clearSearch').click(function () {
            $('#productSearch').val('');
            searchTerm = '';
            currentPage = 1;
            $(this).hide();
            loadProducts();
        });

        // Sort change
        $('#sortSelect').change(function () {
            const val = $(this).val().split('-');
            sortBy = val[0];
            sortOrder = val[1];
            currentPage = 1;
            loadProducts();
        });

        // Page size change
        $('#pageSizeSelect').change(function () {
            pageSize = parseInt($(this).val());
            currentPage = 1;
            loadProducts();
        });

        // Category change (dropdown)
        $('#categorySelect').change(function () {
            selectedCategoryId = $(this).val();
            updateTopMenuSelection(selectedCategoryId);
            currentPage = 1;
            loadProducts();
        });

        // Top Menu Category Clicks
        $(document).on('click', '.dynamic-category-link', function (e) {
            e.preventDefault();
            selectedCategoryId = $(this).data('category-id');

            // Sync with dropdown
            $('#categorySelect').val(selectedCategoryId);

            updateTopMenuSelection(selectedCategoryId);

            currentPage = 1;
            loadProducts();

            // Scroll to products if not there
            $('html, body').animate({
                scrollTop: $('#products').offset().top - 100
            }, 500);
        });

        // "All Products" in top menu
        $(document).on('click', '[asp-action="Index"][data-category=""]', function (e) {
            if ($(this).hasClass('nav-link')) {
                selectedCategoryId = '';
                $('#categorySelect').val('');
                updateTopMenuSelection('');
                currentPage = 1;
                loadProducts();
            }
        });

        $('.btn-view').click(function () {
            $('.btn-view').removeClass('active');
            $(this).addClass('active');
            currentView = $(this).data('view');
            toggleView();
        });

        // Pagination clicks
        $(document).on('click', '#pagination .page-link', function (e) {
            e.preventDefault();
            const page = parseInt($(this).data('page'));
            if (page && page !== currentPage) {
                currentPage = page;
                loadProducts();
                $('html, body').animate({
                    scrollTop: $('#products').offset().top - 100
                }, 500);
            }
        });

        // Quick view button
        $(document).on('click', '.btn-quick-view', function () {
            const productId = $(this).data('id');
            openQuickView(productId);
        });

        // Add to cart button
        $(document).on('click', '.btn-add-to-cart', function () {
            const productId = $(this).data('product-id');
            const qtyInput = $('#quickViewQty');
            const quantity = (qtyInput.length > 0 && $('#quickViewModal').hasClass('show')) ? parseInt(qtyInput.val()) : 1;
            addToCart(productId, quantity);
        });

        // Cart page events
        if ($('#cartItems').length > 0) {
            renderCartPage();
        }

        $(document).on('click', '.btn-remove-cart', function () {
            const id = $(this).data('id');
            removeFromCart(id);
        });

        $(document).on('click', '.btn-qty-minus', function () {
            const id = $(this).data('id');
            updateCartQuantity(id, -1);
        });

        $(document).on('click', '.btn-qty-plus', function () {
            const id = $(this).data('id');
            updateCartQuantity(id, 1);
        });

        $('.btn-checkout').click(function () {
            window.location.href = '/Ecommerce/Shop/Checkout';
        });

        // Checkout page events
        if ($('#checkoutForm').length > 0) {
            renderCheckoutSummary();

            $('#checkoutForm').submit(function (e) {
                e.preventDefault();
                processCheckout();
            });
        }
    }

    // ========================================
    // Data Loading Functions
    // ========================================
    function loadCategories() {
        $.ajax({
            url: '/Ecommerce/Shop/Categories',
            type: 'GET',
            success: function (response) {
                if (response && Array.isArray(response)) {
                    const select = $('#categorySelect');
                    if (select.length > 0) {
                        response.forEach(function (category) {
                            select.append(`<option value="${category.id}">${category.name}</option>`);
                        });

                        // Set initial selection if exists
                        if (selectedCategoryId) {
                            select.val(selectedCategoryId);
                        }
                    }
                }
            }
        });
    }

    function loadProducts() {
        $('#loadingState').show();
        $('#productsGrid').hide();
        $('#emptyState').hide();

        const params = {
            PageNumber: currentPage,
            PageSize: pageSize,
            SortBy: sortBy,
            SortOrder: sortOrder
        };

        if (searchTerm) params.SearchTerm = searchTerm;
        if (selectedCategoryId) params.CategoryId = selectedCategoryId;

        $.ajax({
            url: '/Ecommerce/Shop/Products',
            type: 'GET',
            data: params,
            success: function (response) {
                $('#loadingState').hide();
                if (response.success) {
                    renderProducts(response.data);
                }
            },
            error: function () {
                $('#loadingState').hide();
                $('#emptyState').show();
            }
        });
    }

    // ========================================
    // Rendering Functions
    // ========================================
    function renderProducts(result) {
        const container = $('#productsGrid');
        container.empty();

        if (!result.items || result.items.length === 0) {
            $('#productsGrid').hide();
            $('#emptyState').show();
            $('#pagination').empty();
            $('#resultsInfo span').text('No products found');
            return;
        }

        $('#productsGrid').show();
        $('#emptyState').hide();

        result.items.forEach(function (product) {
            const stockStatus = getStockStatus(product.stock);
            const discountPrice = (product.price * 1.25).toFixed(2);
            const randomRating = (4 + Math.random()).toFixed(1);
            const randomReviews = Math.floor(Math.random() * 200) + 10;

            const card = `
                <div class="product-card h-100 shadow-sm border-0 transition-hover ${currentView === 'list' ? 'list-view' : ''}">
                    <div class="product-image position-relative overflow-hidden" style="height: ${currentView === 'list' ? '150px' : '250px'}; min-width: ${currentView === 'list' ? '200px' : 'auto'};">
                        <img src="https://picsum.photos/seed/${product.id}/400/400" class="card-img-top h-100 w-100 object-fit-cover" alt="${escapeHtml(product.name)}" loading="lazy" />
                        
                        <div class="product-badges position-absolute top-0 start-0 m-2">
                            ${product.stock <= 5 && product.stock > 0 ? '<span class="badge bg-warning text-dark"><i class="bi bi-exclamation-triangle-fill me-1"></i>Low Stock</span>' : ''}
                            ${product.stock === 0 ? '<span class="badge bg-danger"><i class="bi bi-x-circle-fill me-1"></i>Out of Stock</span>' : ''}
                            ${product.stock > 5 ? '<span class="badge bg-success shadow-sm"><i class="bi bi-check-circle-fill me-1"></i>In Stock</span>' : ''}
                        </div>

                        <div class="product-actions-overlay position-absolute bottom-0 start-0 w-100 p-3 bg-gradient-dark d-flex justify-content-center gap-2 translate-y-100 transition-all">
                            <button class="btn btn-light btn-sm rounded-circle shadow-sm btn-quick-view" data-id="${product.id}" title="Quick View">
                                <i class="bi bi-eye"></i>
                            </button>
                            <button class="btn btn-primary btn-sm rounded-circle shadow-sm btn-add-to-cart" data-product-id="${product.id}" title="Add to Cart" ${product.stock === 0 ? 'disabled' : ''}>
                                <i class="bi bi-cart-plus"></i>
                            </button>
                        </div>
                    </div>
                    
                    <div class="card-body p-3">
                        <span class="text-muted small text-uppercase mb-1 d-block">${product.categoryName || 'General'}</span>
                        <h5 class="card-title mb-2 text-truncate">
                            <a href="/Ecommerce/Shop/ProductDetail/${product.id}" class="text-decoration-none text-dark fw-bold">${escapeHtml(product.name)}</a>
                        </h5>
                        
                        <div class="d-flex align-items-center mb-2">
                            <div class="text-warning small me-2">
                                <i class="bi bi-star-fill"></i><i class="bi bi-star-fill"></i><i class="bi bi-star-fill"></i><i class="bi bi-star-fill"></i><i class="bi bi-star-half"></i>
                            </div>
                            <span class="text-muted small">(${randomReviews})</span>
                        </div>

                        <div class="d-flex align-items-center gap-2">
                            <span class="fs-5 fw-bold text-primary">$${product.price.toFixed(2)}</span>
                            <span class="text-muted text-decoration-line-through small text-opacity-50">$${discountPrice}</span>
                        </div>
                        
                        <div class="mt-2 ${stockStatus.class} small fw-bold">
                            <i class="bi ${stockStatus.icon} me-1"></i> ${stockStatus.text}
                        </div>
                    </div>
                </div>
            `;
            container.append(card);
        });

        renderPagination(result);
        updateResultsInfo(result);
        toggleView();
    }

    function renderPagination(result) {
        const pagination = $('#pagination');
        pagination.empty();

        if (result.totalPages <= 1) return;

        const prevDisabled = !result.hasPrevious ? 'disabled' : '';
        pagination.append(`
            <li class="page-item ${prevDisabled}">
                <a class="page-link" href="#" data-page="${result.pageNumber - 1}">
                    <i class="bi bi-chevron-left"></i>
                </a>
            </li>
        `);

        const startPage = Math.max(1, result.pageNumber - 2);
        const endPage = Math.min(result.totalPages, result.pageNumber + 2);

        if (startPage > 1) {
            pagination.append(`
                <li class="page-item">
                    <a class="page-link" href="#" data-page="1">1</a>
                </li>
            `);
            if (startPage > 2) {
                pagination.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            const active = i === result.pageNumber ? 'active' : '';
            pagination.append(`
                <li class="page-item ${active}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                </li>
            `);
        }

        if (endPage < result.totalPages) {
            if (endPage < result.totalPages - 1) {
                pagination.append('<li class="page-item disabled"><span class="page-link">...</span></li>');
            }
            pagination.append(`
                <li class="page-item">
                    <a class="page-link" href="#" data-page="${result.totalPages}">${result.totalPages}</a>
                </li>
            `);
        }

        const nextDisabled = !result.hasNext ? 'disabled' : '';
        pagination.append(`
            <li class="page-item ${nextDisabled}">
                <a class="page-link" href="#" data-page="${result.pageNumber + 1}">
                    <i class="bi bi-chevron-right"></i>
                </a>
            </li>
        `);
    }

    function updateResultsInfo(result) {
        const start = (result.pageNumber - 1) * result.pageSize + 1;
        const end = Math.min(result.pageNumber * result.pageSize, result.totalCount);
        $('#resultsInfo span').text(`Showing ${start}-${end} of ${result.totalCount} products`);
    }

    // ========================================
    // Quick View
    // ========================================
    function openQuickView(productId) {
        $.ajax({
            url: `/Ecommerce/Shop/Products?PageSize=1000`,
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    const product = response.data.items.find(p => p.id === productId);
                    if (product) {
                        $('#quickViewName').text(product.name);
                        $('#quickViewCategory').text(product.categoryName || 'General');
                        $('#quickViewPrice').text('$' + product.price.toFixed(2));
                        $('#quickViewOldPrice').text('$' + (product.price * 1.25).toFixed(2));
                        $('#quickViewDescription').text(product.description);

                        const stockStatus = getStockStatus(product.stock);
                        $('#quickViewStock').html(`<i class="bi ${stockStatus.icon} me-1"></i> ${stockStatus.text}`)
                            .attr('class', 'fw-bold ' + stockStatus.class);

                        $('#quickViewImage').attr('src', `https://picsum.photos/seed/${product.id}/600/600`);
                        $('#quickViewQty').val(1);

                        $('.btn-add-cart-modal').attr('onclick', `addToCart(${product.id})`)
                            .prop('disabled', product.stock === 0);

                        $('#quickViewModal').modal('show');
                    }
                }
            }
        });
    }

    // ========================================
    // Cart Functions
    // ========================================
    function addToCart(productId, quantity = 1) {
        let cart = JSON.parse(localStorage.getItem('shopCart') || '[]');
        const existingItem = cart.find(item => item.id === productId);

        if (existingItem) {
            existingItem.quantity += quantity;
        } else {
            cart.push({ id: productId, quantity: quantity });
        }

        localStorage.setItem('shopCart', JSON.stringify(cart));
        updateCartCount();

        showToast('Product added to cart!');
        if ($('#quickViewModal').hasClass('show')) {
            $('#quickViewModal').modal('hide');
        }
    }

    function removeFromCart(productId) {
        let cart = JSON.parse(localStorage.getItem('shopCart') || '[]');
        cart = cart.filter(item => item.id !== productId);
        localStorage.setItem('shopCart', JSON.stringify(cart));
        updateCartCount();
        renderCartPage();
    }

    function updateCartQuantity(productId, change) {
        let cart = JSON.parse(localStorage.getItem('shopCart') || '[]');
        const item = cart.find(item => item.id === productId);
        if (item) {
            item.quantity += change;
            if (item.quantity <= 0) {
                removeFromCart(productId);
            } else {
                localStorage.setItem('shopCart', JSON.stringify(cart));
                updateCartCount();
                renderCartPage();
            }
        }
    }

    function updateCartCount() {
        const cart = JSON.parse(localStorage.getItem('shopCart') || '[]');
        const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);
        $('.cart-count').text(totalItems);
    }

    async function renderCartPage() {
        const cart = JSON.parse(localStorage.getItem('shopCart') || '[]');
        const container = $('#cartItems');
        const emptyState = $('#cartEmpty');
        const summary = $('#cartSummary');

        if (cart.length === 0) {
            container.hide();
            summary.hide();
            emptyState.show();
            return;
        }

        emptyState.hide();
        container.show();
        summary.show();
        container.html('<div class="text-center p-5"><div class="spinner-border text-primary"></div></div>');

        try {
            // Get product details for cart items
            const response = await $.ajax({ url: '/Ecommerce/Shop/Products?PageSize=1000', type: 'GET' });
            if (response.success) {
                const products = response.data.items;
                let html = '';
                let subtotal = 0;

                cart.forEach(item => {
                    const product = products.find(p => p.id === item.id);
                    if (product) {
                        const total = product.price * item.quantity;
                        subtotal += total;
                        html += `
                            <div class="cart-item d-flex align-items-center p-3 mb-3 border rounded shadow-sm">
                                <img src="https://picsum.photos/seed/${product.id}/100/100" class="rounded me-3" style="width: 80px; height: 80px; object-fit: cover;">
                                <div class="flex-grow-1">
                                    <h5 class="mb-1">${escapeHtml(product.name)}</h5>
                                    <p class="text-muted small mb-0">${product.categoryName || 'General'}</p>
                                    <div class="fw-bold text-primary">$${product.price.toFixed(2)}</div>
                                </div>
                                <div class="quantity-controls d-flex align-items-center me-4">
                                    <button class="btn btn-sm btn-outline-secondary btn-qty-minus" data-id="${product.id}"><i class="bi bi-dash"></i></button>
                                    <span class="mx-3 fw-bold">${item.quantity}</span>
                                    <button class="btn btn-sm btn-outline-secondary btn-qty-plus" data-id="${product.id}"><i class="bi bi-plus"></i></button>
                                </div>
                                <div class="text-end me-4" style="min-width: 100px;">
                                    <div class="fw-bold fs-5">$${total.toFixed(2)}</div>
                                </div>
                                <button class="btn btn-link text-danger btn-remove-cart" data-id="${product.id}"><i class="bi bi-trash fs-4"></i></button>
                            </div>
                        `;
                    }
                });

                container.html(html);
                const tax = subtotal * 0.10;
                const total = subtotal + tax;

                $('#cartSubtotal').text('$' + subtotal.toFixed(2));
                $('#cartTax').text('$' + tax.toFixed(2));
                $('#cartTotal').text('$' + total.toFixed(2));
            }
        } catch (error) {
            console.error('Error rendering cart:', error);
            container.html('<div class="alert alert-danger">Error loading cart items</div>');
        }
    }

    async function renderCheckoutSummary() {
        const cart = JSON.parse(localStorage.getItem('shopCart') || '[]');
        const container = $('#checkoutSummaryItems');

        if (cart.length === 0) {
            window.location.href = '/Ecommerce/Shop/Cart';
            return;
        }

        try {
            const response = await $.ajax({ url: '/Ecommerce/Shop/Products?PageSize=1000', type: 'GET' });
            if (response.success) {
                const products = response.data.items;
                let html = '';
                let subtotal = 0;

                cart.forEach(item => {
                    const product = products.find(p => p.id === item.id);
                    if (product) {
                        const total = product.price * item.quantity;
                        subtotal += total;
                        html += `
                            <div class="d-flex justify-content-between align-items-center mb-3">
                                <div>
                                    <h6 class="mb-0">${escapeHtml(product.name)}</h6>
                                    <small class="text-muted">${item.quantity} x $${product.price.toFixed(2)}</small>
                                </div>
                                <span class="fw-bold">$${total.toFixed(2)}</span>
                            </div>
                        `;
                    }
                });

                container.html(html);
                const tax = subtotal * 0.10;
                const total = subtotal + tax;

                $('#checkoutSubtotal').text('$' + subtotal.toFixed(2));
                $('#checkoutTax').text('$' + tax.toFixed(2));
                $('#checkoutTotal').text('$' + total.toFixed(2));

                // Also update the ones in footer (if different IDs used)
                $('#checkoutSubtotal, #cartSubtotal').text('$' + subtotal.toFixed(2));
                $('#checkoutTax, #cartTax').text('$' + tax.toFixed(2));
                $('#checkoutTotal, #cartTotal').text('$' + total.toFixed(2));
            }
        } catch (error) {
            console.error('Error rendering checkout summary:', error);
        }
    }

    function processCheckout() {
        const cart = JSON.parse(localStorage.getItem('shopCart') || '[]');
        if (cart.length === 0) return;

        const orderData = {
            customerName: $('#customerName').val(),
            email: $('#email').val(),
            phone: $('#phone').val(),
            address: $('#address').val(),
            city: $('#city').val(),
            zipCode: $('#zipCode').val(),
            orderItems: cart.map(item => ({
                productId: item.id,
                quantity: item.quantity
            }))
        };

        $('#btnPlaceOrder').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-2"></span>Processing...');

        $.ajax({
            url: '/Ecommerce/Shop/PlaceOrder',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(orderData),
            success: function (response) {
                if (response.success) {
                    localStorage.removeItem('shopCart');
                    window.location.href = `/Ecommerce/Shop/Confirmation/${response.orderId}`;
                } else {
                    alert('Error: ' + (response.message || 'Failed to place order'));
                    $('#btnPlaceOrder').prop('disabled', false).html('<i class="bi bi-lock-fill me-1"></i> Place Order Now');
                }
            },
            error: function () {
                alert('An error occurred while placing your order.');
                $('#btnPlaceOrder').prop('disabled', false).html('<i class="bi bi-lock-fill me-1"></i> Place Order Now');
            }
        });
    }

    // ========================================
    // Utility Functions
    // ========================================
    function getStockStatus(stock) {
        if (stock === 0) {
            return { class: 'out-of-stock', icon: 'bi-x-circle', text: 'Out of Stock' };
        } else if (stock <= 5) {
            return { class: 'low-stock', icon: 'bi-exclamation-circle', text: `Only ${stock} left` };
        } else {
            return { class: 'in-stock', icon: 'bi-check-circle', text: 'In Stock' };
        }
    }

    function toggleView() {
        const grid = $('#productsGrid');
        if (currentView === 'list') {
            grid.css('grid-template-columns', '1fr');
            grid.find('.product-card').addClass('list-view');
        } else {
            grid.css('grid-template-columns', '');
            grid.find('.product-card').removeClass('list-view');
        }
    }

    function showToast(message) {
        // Simple toast notification
        const toast = $(`
            <div class="toast-notification">
                <i class="bi bi-check-circle-fill"></i>
                <span>${message}</span>
            </div>
        `).appendTo('body');

        setTimeout(() => toast.addClass('show'), 10);
        setTimeout(() => {
            toast.removeClass('show');
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    function escapeHtml(text) {
        if (!text) return '';
        const map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' };
        return text.replace(/[&<>"']/g, function (m) { return map[m]; });
    }

    // Quick View quantity controls
    window.incrementQty = function () {
        const input = $('#quickViewQty');
        input.val(parseInt(input.val()) + 1);
    };

    window.decrementQty = function () {
        const input = $('#quickViewQty');
        const val = parseInt(input.val());
        if (val > 1) input.val(val - 1);
    };

    // Clear filters function
    window.clearFilters = function () {
        searchTerm = '';
        currentPage = 1;
        sortBy = 'Name';
        sortOrder = 'asc';
        selectedCategoryId = '';
        $('#productSearch').val('');
        $('#sortSelect').val('Name-asc');
        $('#categorySelect').val('');
        updateTopMenuSelection('');
        $('#clearSearch').hide();
        loadProducts();
    };

    function updateTopMenuSelection(categoryId) {
        $('.nav-link').removeClass('active');
        if (!categoryId) {
            $('[asp-action="Index"][data-category=""]').addClass('active');
        } else {
            $(`.dynamic-category-link[data-category-id="${categoryId}"]`).addClass('active');
        }
    }

    // Initialize cart count
    updateCartCount();
});

// Toast notification styles (added dynamically)
$('<style>').text(`
    .toast-notification {
        position: fixed;
        bottom: 30px;
        right: 30px;
        background: #10b981;
        color: #fff;
        padding: 1rem 1.5rem;
        border-radius: 8px;
        display: flex;
        align-items: center;
        gap: 0.75rem;
        box-shadow: 0 4px 20px rgba(0,0,0,0.2);
        transform: translateX(120%);
        transition: transform 0.3s ease;
        z-index: 9999;
    }
    .toast-notification.show {
        transform: translateX(0);
    }
    .toast-notification i {
        font-size: 1.25rem;
    }
`).appendTo('head');
