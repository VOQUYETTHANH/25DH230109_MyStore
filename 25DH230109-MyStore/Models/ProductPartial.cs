using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace _25DH230109_MyStore.Models
{
    // Thuộc tính hỗ trợ upload ảnh, không được ánh xạ thành cột trong cơ sở dữ liệu.
    public partial class Product
    {
        [NotMapped]
        public HttpPostedFileBase UploadImg { get; set; }
    }
}
