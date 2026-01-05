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

    // Initialize
    loadProducts();
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

        // View toggle
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
            addToCart(productId);
        });
    }

    // ========================================
    // Data Loading Functions
    // ========================================
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
            const discountPrice = (product.price * 1.2).toFixed(2);
            const randomRating = (4 + Math.random()).toFixed(1);
            const randomReviews = Math.floor(Math.random() * 200) + 10;

            const card = `
                <div class="product-card">
                    <div class="product-image">
                        <img src="https://picsum.photos/seed/${product.id}/400/400" alt="${escapeHtml(product.name)}" loading="lazy" />
                        <div class="product-badges">
                            ${product.stock <= 5 && product.stock > 0 ? '<span class="badge bg-warning">Low Stock</span>' : ''}
                            ${product.stock === 0 ? '<span class="badge bg-danger">Sold Out</span>' : ''}
                        </div>
                        <div class="product-actions-overlay">
                            <button class="btn btn-quick-view" data-id="${product.id}" title="Quick View">
                                <i class="bi bi-eye"></i>
                            </button>
                            <button class="btn" title="Add to Wishlist">
                                <i class="bi bi-heart"></i>
                            </button>
                            <button class="btn btn-add-to-cart" data-product-id="${product.id}" title="Add to Cart" ${product.stock === 0 ? 'disabled' : ''}>
                                <i class="bi bi-cart-plus"></i>
                            </button>
                        </div>
                    </div>
                    <div class="product-info">
                        <span class="product-category">General</span>
                        <h3 class="product-name">
                            <a href="/Ecommerce/Shop/ProductDetail/${product.id}">${escapeHtml(product.name)}</a>
                        </h3>
                        <div class="product-rating">
                            <i class="bi bi-star-fill"></i>
                            <i class="bi bi-star-fill"></i>
                            <i class="bi bi-star-fill"></i>
                            <i class="bi bi-star-fill"></i>
                            <i class="bi bi-star-half"></i>
                            <span>(${randomRating}) ${randomReviews} reviews</span>
                        </div>
                        <div class="product-price">
                            <span class="current-price">$${product.price.toFixed(2)}</span>
                            <span class="original-price">$${discountPrice}</span>
                        </div>
                        <div class="product-stock ${stockStatus.class}">
                            <i class="bi ${stockStatus.icon}"></i> ${stockStatus.text}
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
                        $('#quickViewPrice').text('$' + product.price.toFixed(2));
                        $('#quickViewDescription').text(product.description);
                        $('#quickViewStock').text(product.stock + ' units in stock');
                        $('#quickViewImage').attr('src', `https://picsum.photos/seed/${product.id}/500/500`);
                        $('#quickViewQty').val(1);
                        $('#quickViewModal').modal('show');
                    }
                }
            }
        });
    }

    // ========================================
    // Cart Functions
    // ========================================
    function addToCart(productId) {
        // Simple cart implementation using localStorage
        let cart = JSON.parse(localStorage.getItem('shopCart') || '[]');
        const existingItem = cart.find(item => item.id === productId);
        
        if (existingItem) {
            existingItem.quantity += 1;
        } else {
            cart.push({ id: productId, quantity: 1 });
        }
        
        localStorage.setItem('shopCart', JSON.stringify(cart));
        updateCartCount();
        
        // Show feedback
        showToast('Product added to cart!');
    }

    function updateCartCount() {
        const cart = JSON.parse(localStorage.getItem('shopCart') || '[]');
        const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);
        $('.cart-count').text(totalItems);
    }

    // Initialize cart count
    updateCartCount();

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
    window.incrementQty = function() {
        const input = $('#quickViewQty');
        input.val(parseInt(input.val()) + 1);
    };

    window.decrementQty = function() {
        const input = $('#quickViewQty');
        const val = parseInt(input.val());
        if (val > 1) input.val(val - 1);
    };

    // Clear filters function
    window.clearFilters = function() {
        searchTerm = '';
        currentPage = 1;
        sortBy = 'Name';
        sortOrder = 'asc';
        $('#productSearch').val('');
        $('#sortSelect').val('Name-asc');
        $('#clearSearch').hide();
        loadProducts();
    };
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

