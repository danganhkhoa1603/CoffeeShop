using System.Text;
using CoffeeShop.Models;

namespace CoffeeShop.Extensions
{
    public static class InvoiceExportHelper
    {
        // Tạo file Excel cho một đơn hàng (Hóa đơn chi tiết)
        public static byte[] GenerateInvoiceExcel(Order order, List<OrderDetail> details)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns='http://www.w3.org/TR/REC-html40'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />");
            sb.AppendLine("<!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>Hóa đơn #" + order.OrderId + "</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]-->");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; font-size: 13px; }");
            sb.AppendLine(".title { font-size: 18px; font-weight: bold; color: #6f4e37; text-align: center; }");
            sb.AppendLine(".subtitle { font-size: 12px; color: #555; text-align: center; }");
            sb.AppendLine(".header-table { width: 100%; margin-bottom: 20px; }");
            sb.AppendLine(".header-table td { padding: 4px; font-size: 13px; }");
            sb.AppendLine(".item-table { width: 100%; border-collapse: collapse; margin-top: 15px; }");
            sb.AppendLine(".item-table th { background-color: #6f4e37; color: #ffffff; font-weight: bold; border: 1px solid #4a3222; padding: 8px; text-align: center; }");
            sb.AppendLine(".item-table td { border: 1px solid #ddd; padding: 8px; }");
            sb.AppendLine(".text-center { text-align: center; }");
            sb.AppendLine(".text-end { text-align: right; }");
            sb.AppendLine(".fw-bold { font-weight: bold; }");
            sb.AppendLine(".total-row { background-color: #fdfaf7; font-weight: bold; }");
            sb.AppendLine(".grand-total { background-color: #f5ede6; color: #d9534f; font-weight: bold; font-size: 15px; }");
            sb.AppendLine(".footer-note { font-style: italic; color: #666; margin-top: 20px; text-align: center; font-size: 12px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Store Header
            sb.AppendLine("<table style='width: 100%; margin-bottom: 10px;'>");
            sb.AppendLine("<tr><td colspan='5' class='title'>☕ COFFEE PREMIUM</td></tr>");
            sb.AppendLine("<tr><td colspan='5' class='subtitle'>Địa chỉ: TP. Hồ Chí Minh | Hotline: 0938.282.070 | Email: coffeeshop@gmail.com</td></tr>");
            sb.AppendLine("<tr><td colspan='5' class='title' style='padding-top: 15px;'>HÓA ĐƠN BÁN HÀNG</td></tr>");
            sb.AppendLine($"<tr><td colspan='5' class='subtitle'>Mã đơn hàng: <strong>#{order.OrderId}</strong> | Ngày đặt: {order.OrderDate:dd/MM/yyyy HH:mm:ss}</td></tr>");
            sb.AppendLine("</table>");

            // Customer Info Box
            sb.AppendLine("<table class='header-table' style='border: 1px solid #eee; background-color: #faf6f2;'>");
            sb.AppendLine($"<tr><td style='width: 20%;'><strong>Khách hàng:</strong></td><td>{order.CustomerName}</td><td style='width: 20%;'><strong>Trạng thái:</strong></td><td>{order.Status}</td></tr>");
            sb.AppendLine($"<tr><td><strong>Số điện thoại:</strong></td><td>{order.Phone}</td><td><strong>Ngày in:</strong></td><td>{DateTime.Now:dd/MM/yyyy HH:mm}</td></tr>");
            sb.AppendLine($"<tr><td><strong>Địa chỉ giao:</strong></td><td colspan='3'>{order.Address}</td></tr>");
            if (!string.IsNullOrEmpty(order.Note))
            {
                sb.AppendLine($"<tr><td><strong>Ghi chú / Thanh toán:</strong></td><td colspan='3'>{order.Note}</td></tr>");
            }
            sb.AppendLine("</table>");

            // Order Items Table
            sb.AppendLine("<table class='item-table'>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th style='width: 40px;'>STT</th>");
            sb.AppendLine("<th>Tên sản phẩm / Thức uống</th>");
            sb.AppendLine("<th style='width: 80px;'>Số lượng</th>");
            sb.AppendLine("<th style='width: 120px;'>Đơn giá (VNĐ)</th>");
            sb.AppendLine("<th style='width: 140px;'>Thành tiền (VNĐ)</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            int stt = 1;
            decimal subtotal = 0;
            foreach (var item in details)
            {
                var lineTotal = item.Price * item.Quantity;
                subtotal += lineTotal;
                string productName = item.Product != null ? item.Product.ProductName : "Sản phẩm";

                sb.AppendLine("<tr>");
                sb.AppendLine($"<td class='text-center'>{stt++}</td>");
                sb.AppendLine($"<td><strong>{productName}</strong></td>");
                sb.AppendLine($"<td class='text-center'>{item.Quantity}</td>");
                sb.AppendLine($"<td class='text-end'>{item.Price:N0}</td>");
                sb.AppendLine($"<td class='text-end fw-bold'>{lineTotal:N0}</td>");
                sb.AppendLine("</tr>");
            }

            decimal shippingFee = 20000;
            decimal grandTotal = order.TotalMoney > 0 ? order.TotalMoney : (subtotal + shippingFee);

            sb.AppendLine("<tr class='total-row'>");
            sb.AppendLine("<td colspan='4' class='text-end'>Tạm tính tiền hàng:</td>");
            sb.AppendLine($"<td class='text-end'>{subtotal:N0}</td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("<tr class='total-row'>");
            sb.AppendLine("<td colspan='4' class='text-end'>Phí giao hàng:</td>");
            sb.AppendLine($"<td class='text-end'>{shippingFee:N0}</td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("<tr class='grand-total'>");
            sb.AppendLine("<td colspan='4' class='text-end' style='font-size: 14px;'>TỔNG CỘNG THANH TOÁN (VNĐ):</td>");
            sb.AppendLine($"<td class='text-end' style='font-size: 14px;'>{grandTotal:N0}</td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            // Signatures
            sb.AppendLine("<table style='width: 100%; margin-top: 30px; text-align: center;'>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td style='width: 50%; font-weight: bold;'>KHÁCH HÀNG<br/><span style='font-weight: normal; font-size: 11px;'>(Ký và ghi rõ họ tên)</span></td>");
            sb.AppendLine("<td style='width: 50%; font-weight: bold;'>NGƯỜI LẬP HÓA ĐƠN<br/><span style='font-weight: normal; font-size: 11px;'>(Ký và ghi rõ họ tên)</span><br/><br/><br/><br/><strong>Coffee Premium</strong></td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");

            sb.AppendLine("<p class='footer-note'>❤️ Cảm ơn quý khách đã tin tưởng và ủng hộ Coffee Premium!</p>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            // Return UTF-8 with BOM for Excel Vietnamese compatibility
            var preamble = Encoding.UTF8.GetPreamble();
            var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var result = new byte[preamble.Length + contentBytes.Length];
            Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
            Buffer.BlockCopy(contentBytes, 0, result, preamble.Length, contentBytes.Length);

            return result;
        }

        // Tạo file Excel tổng hợp danh sách đơn hàng cho Admin
        public static byte[] GenerateOrdersListExcel(List<Order> orders, string filterTitle)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:x='urn:schemas-microsoft-com:office:excel' xmlns='http://www.w3.org/TR/REC-html40'>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />");
            sb.AppendLine("<!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>Danh sách đơn hàng</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]-->");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; font-size: 12px; }");
            sb.AppendLine(".title { font-size: 16px; font-weight: bold; color: #6f4e37; text-align: center; }");
            sb.AppendLine(".subtitle { font-size: 11px; color: #555; text-align: center; margin-bottom: 15px; }");
            sb.AppendLine(".order-table { width: 100%; border-collapse: collapse; margin-top: 10px; }");
            sb.AppendLine(".order-table th { background-color: #6f4e37; color: #ffffff; font-weight: bold; border: 1px solid #4a3222; padding: 8px; text-align: center; font-size: 12px; }");
            sb.AppendLine(".order-table td { border: 1px solid #ddd; padding: 6px 8px; font-size: 12px; }");
            sb.AppendLine(".text-center { text-align: center; }");
            sb.AppendLine(".text-end { text-align: right; }");
            sb.AppendLine(".fw-bold { font-weight: bold; }");
            sb.AppendLine(".total-row { background-color: #f5ede6; font-weight: bold; font-size: 13px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            sb.AppendLine("<table style='width: 100%; margin-bottom: 10px;'>");
            sb.AppendLine("<tr><td colspan='8' class='title'>☕ COFFEE PREMIUM - BÁO CÁO DANH SÁCH ĐƠN HÀNG</td></tr>");
            sb.AppendLine($"<tr><td colspan='8' class='subtitle'>Bộ lọc: {filterTitle} | Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</td></tr>");
            sb.AppendLine("</table>");

            sb.AppendLine("<table class='order-table'>");
            sb.AppendLine("<thead>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<th style='width: 40px;'>STT</th>");
            sb.AppendLine("<th style='width: 80px;'>Mã đơn</th>");
            sb.AppendLine("<th style='width: 160px;'>Khách hàng</th>");
            sb.AppendLine("<th style='width: 110px;'>Số điện thoại</th>");
            sb.AppendLine("<th style='width: 220px;'>Địa chỉ giao hàng</th>");
            sb.AppendLine("<th style='width: 130px;'>Ngày đặt</th>");
            sb.AppendLine("<th style='width: 130px;'>Tổng tiền (VNĐ)</th>");
            sb.AppendLine("<th style='width: 110px;'>Trạng thái</th>");
            sb.AppendLine("<th style='width: 180px;'>Ghi chú & Phương thức</th>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</thead>");
            sb.AppendLine("<tbody>");

            int stt = 1;
            decimal totalRevenue = 0;
            foreach (var item in orders)
            {
                totalRevenue += item.TotalMoney;
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td class='text-center'>{stt++}</td>");
                sb.AppendLine($"<td class='text-center fw-bold'>#{item.OrderId}</td>");
                sb.AppendLine($"<td><strong>{item.CustomerName}</strong></td>");
                sb.AppendLine($"<td>{item.Phone}</td>");
                sb.AppendLine($"<td>{item.Address}</td>");
                sb.AppendLine($"<td class='text-center'>{item.OrderDate:dd/MM/yyyy HH:mm}</td>");
                sb.AppendLine($"<td class='text-end fw-bold'>{item.TotalMoney:N0}</td>");
                sb.AppendLine($"<td class='text-center'>{item.Status}</td>");
                sb.AppendLine($"<td>{item.Note}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("<tr class='total-row'>");
            sb.AppendLine($"<td colspan='6' class='text-end'>TỔNG CỘNG ({orders.Count} đơn hàng):</td>");
            sb.AppendLine($"<td class='text-end' style='color: #cf1322;'>{totalRevenue:N0}</td>");
            sb.AppendLine("<td colspan='2'></td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            var preamble = Encoding.UTF8.GetPreamble();
            var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var result = new byte[preamble.Length + contentBytes.Length];
            Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
            Buffer.BlockCopy(contentBytes, 0, result, preamble.Length, contentBytes.Length);

            return result;
        }
    }
}
