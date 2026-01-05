// Admin Panel - Common JavaScript
$(document).ready(function() {
    // Sidebar toggle for mobile
    $('.sidebar-toggle').click(function() {
        $('.admin-sidebar').toggleClass('show');
    });

    // Close sidebar when clicking outside on mobile
    $(document).click(function(e) {
        if ($(window).width() < 992) {
            if (!$(e.target).closest('.admin-sidebar').length && !$(e.target).closest('.sidebar-toggle').length) {
                $('.admin-sidebar').removeClass('show');
            }
        }
    });

    // Active nav item highlighting based on current URL
    const currentPath = window.location.pathname.toLowerCase();
    $('.sidebar-nav .nav-link').each(function() {
        const linkPath = $(this).attr('href');
        if (linkPath && currentPath.includes(linkPath.toLowerCase())) {
            $(this).addClass('active');
        }
    });
});

