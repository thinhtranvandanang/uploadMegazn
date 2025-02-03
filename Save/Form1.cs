using CG.Web.MegaApiClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Save
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private async void btnUpload_Click(object sender, EventArgs e)
        {
            btnUpload.Enabled = false;
            
            // Thông tin đăng nhập
            string email = clsCommon.MegaEmailLogin;
            string password = clsCommon.MegaPassLogin;
            // Khởi tạo MegaApiClient
            var client = new MegaApiClient();
            client.Login(email, password);
            // Đường dẫn tệp cần tải lên
            string filePath = clsCommon.Test_FileUpload;
            string targetFolderName = clsCommon.Test_DirUpload; // Tên thư mục chỉ định trên Mega
            // Kiểm tra xem tệp có tồn tại không
            if (File.Exists(filePath))
            {
                // Lấy danh sách các node
                var nodes = client.GetNodes();
                string fileName = Path.GetFileName(filePath);
                int retryCount = 3; // Số lần thử lại
                                    // Tìm thư mục chỉ định
                                    // In ra danh sách các node để kiểm tra
               // foreach (var node in nodes)
               // {
               //     MessageBox.Show($"Node: {node.Name}, Type: {node.Type}, ParentId: {node.ParentId}");
               // }
                var targetFolder = nodes.FirstOrDefault(n => n.Type == NodeType.Directory && n.Name == targetFolderName);
                if (targetFolder == null)   // Nếu thư mục không tồn tại, tạo mới
                {
                    // Lấy root node
                    var rootNode = nodes.FirstOrDefault(n => n.Type == NodeType.Root);                    // Kiểm tra xem rootNode có tồn tại không
                    if (rootNode == null)
                    {
                        MessageBox.Show("Không tìm thấy thư mục gốc.");
                        btnUpload.Enabled = true;
                        return;
                    }

                    // Tạo thư mục mới
                    try
                    {
                        targetFolder = await client.CreateFolderAsync(targetFolderName, rootNode);
                        MessageBox.Show($"Thư mục '{targetFolderName}' đã được tạo.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi tạo thư mục: {ex.Message}");
                        btnUpload.Enabled = true;
                        return;
                    }
                }
                else // nếu thư mục đã có
                {
                    // Kiểm tra xem tệp đã tồn tại trong thư mục chỉ định
                    var existingFile = nodes.FirstOrDefault(n => n.Type == NodeType.File && n.Name == fileName && n.ParentId == targetFolder.Id);
                    if (existingFile != null)
                    {
                        // Hỏi người dùng có muốn ghi đè không
                        var result = MessageBox.Show($"Tệp '{fileName}' đã tồn tại trong thư mục '{targetFolderName}'. Bạn có muốn ghi đè không?",
                            "Xác nhận ghi đè", MessageBoxButtons.YesNo);
                        if (result == DialogResult.No)
                        {
                            // Nếu người dùng chọn không, thoát
                            return;
                        }
                    }
                }
                    /// upload file
                    using (var stream = File.OpenRead(filePath)) // Mở tệp dưới dạng stream
                    {
                        // Tải lên tệp với xử lý thử lại
                        for (int i = 0; i < retryCount; i++)
                        {
                            try
                            {
                                // Tạo một ProgressReporter để theo dõi tiến độ
                                var progressReporter = new Progress<double>(percent =>
                                {
                                    progressBarUpload.Value = (int)percent; // Cập nhật giá trị ProgressBar
                                });

                                // Tải lên tệp với báo cáo tiến độ
                                var node = await client.UploadAsync(stream, Path.GetFileName(filePath), targetFolder, progressReporter);
                                MessageBox.Show($"Tải lên thành công! Tệp đã được lưu với ID: {node.Id}");
                                break; // Thoát khỏi vòng lặp nếu thành công
                            }
                            catch (ApiException ex)
                            {
                                MessageBox.Show($"Lỗi: {ex.Message}. Thử lại lần {i + 1}...");
                                await Task.Delay(2000); // Đợi 2 giây trước khi thử lại
                            }
                        }
                    } /// upload file 
            }
             else
            {
               MessageBox.Show("Tệp không tồn tại trên máy tính!");
            }
                // Đăng xuất
                client.Logout();
            btnUpload.Enabled = true;
        }







        }
}
