$(document).ready(function () {
    loadOrders();

    $('#btnFilter').click(function() {
        loadOrders();
    });

    $('#btnClearSearch').click(function() {
        $('#searchBox').val('');
        $('#statusFilter').val('');
        loadOrders();
    });

    $('#searchBox').on('keypress', function(e) {
        if (e.which === 13) {
            loadOrders();
        }
    });
});

function loadOrders() {
    const search = $('#searchBox').val().toLowerCase();
    const status = $('#statusFilter').val();
    
    $('#ordersTableBody').hide();
    $('#loadingIndicator').show();
    $('#emptyState').hide();

    $.ajax({
        url: window.adminAreaPrefix + '/Orders/GetAll',
        type: 'GET',
        success: function (response) {
            $('#loadingIndicator').hide();
            if (response.success) {
                let filteredData = response.data;

                if (search) {
                    filteredData = filteredData.filter(o => 
                        o.orderNumber.toLowerCase().includes(search) || 
                        o.customerName.toLowerCase().includes(search)
                    );
                }

                if (status) {
                    // status is string name of enum from select
                    filteredData = filteredData.filter(o => getStatusName(o.status) === status);
                }

                renderOrders(filteredData);
                updateStats(response.data);
            }
        },
        error: function () {
            $('#loadingIndicator').hide();
            console.error('Error loading orders');
        }
    });
}

function renderOrders(orders) {
    const $tbody = $('#ordersTableBody');
    $tbody.empty();

    if (orders.length === 0) {
        $('#emptyState').show();
        return;
    }

    orders.forEach(order => {
        const date = new Date(order.orderDate).toLocaleDateString();
        const statusBadge = getStatusBadge(order.status);
        
        const row = `
            <tr>
                <td class="ps-4">
                    <span class="fw-bold text-dark">${order.orderNumber}</span>
                </td>
                <td><span class="text-muted small">${date}</span></td>
                <td>
                    <div class="d-flex align-items-center">
                        <div>
                            <span class="fw-medium d-block">${order.customerName}</span>
                            <span class="text-muted small">${order.email}</span>
                        </div>
                    </div>
                </td>
                <td class="text-end fw-bold text-primary">${formatCurrency(order.totalAmount)}</td>
                <td class="text-center">${statusBadge}</td>
                <td class="pe-4 text-end">
                    <div class="btn-group">
                        <a href="/Admin/Orders/Details/${order.id}" class="btn btn-sm btn-outline-primary rounded-pill px-3">
                            <i class="fas fa-eye me-1"></i> Details
                        </a>
                    </div>
                </td>
            </tr>
        `;
        $tbody.append(row);
    });

    $tbody.show();
}

function updateStats(orders) {
    $('#statTotalOrders').text(orders.length);
    $('#statPendingOrders').text(orders.filter(o => o.status === 0).length);
    $('#statDeliveredOrders').text(orders.filter(o => o.status === 3).length);
    
    const revenue = orders.reduce((sum, o) => sum + o.totalAmount, 0);
    $('#statTotalRevenue').text(formatCurrency(revenue));
}

function getStatusBadge(status) {
    const name = getStatusName(status);
    let bg = 'bg-secondary';
    
    switch(status) {
        case 0: bg = 'bg-warning'; break; // Pending
        case 1: bg = 'bg-info'; break;    // Processing
        case 2: bg = 'bg-primary'; break; // Shipped
        case 3: bg = 'bg-success'; break; // Delivered
        case 4: bg = 'bg-danger'; break;  // Cancelled
        case 5: bg = 'bg-dark'; break;    // Refunded
    }
    
    return `<span class="badge ${bg} bg-opacity-10 text-${bg.replace('bg-', '')} border border-${bg.replace('bg-', '')} border-opacity-25 rounded-pill px-3 py-2">${name}</span>`;
}

function getStatusName(status) {
    const statuses = ['Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled', 'Refunded'];
    return statuses[status] || 'Unknown';
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);
}
