using System.Collections.Generic;
using PagedList;
using _25DH230109_MyStore.Models;

namespace _25DH230109_MyStore.Models.ViewModel
{
    // ViewModel dùng cho tìm kiếm, sắp xếp và phân trang sản phẩm theo nội dung PPT.
    public class ProductSearchVM
    {
        public string Keyword { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortOrder { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public IPagedList<Product> Products { get; set; }
    }
}
