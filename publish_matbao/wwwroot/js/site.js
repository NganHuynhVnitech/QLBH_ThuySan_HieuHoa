// Global JavaScript for QLBH_ThuySan

// Format number to Vietnamese standard: 1.000.000
function formatNumber(n) {
    if (n === null || n === undefined || n === "" || n === 0) return "0";
    var num = parseFloat(n);
    if (isNaN(num)) return "0";
    return num.toLocaleString('vi-VN', { minimumFractionDigits: 0, maximumFractionDigits: 0 });
}

// Format quantity/decimal to Vietnamese standard: 1.234,56
function formatDecimal(n, decimals = 2) {
    if (n === null || n === undefined || n === "" || n === 0) return "0";
    var num = parseFloat(n);
    if (isNaN(num)) return "0";
    return num.toLocaleString('vi-VN', { 
        minimumFractionDigits: 0, 
        maximumFractionDigits: decimals 
    });
}
