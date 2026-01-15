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

        function updateTopMenuSelection(categoryId) {
            $('.nav-link').removeClass('active');
            if (!categoryId) {
                $('[asp-action="Index"][data-category=""]').addClass('active');
            } else {
                $(`.dynamic-category-link[data-category-id="${categoryId}"]`).addClass('active');
            }
        }
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

