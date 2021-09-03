using Microsoft.ML.Data;

namespace Vk.Post.Predict.Models
{
    public class VkMessageML
    {
        [LoadColumn(0)]
        public int OwnerId { get; set; }
        [LoadColumn(1)]
        public int Id { get; set; }
        
        [LoadColumn(2)]
        public string OwnerName { get; set; }

        [LoadColumn(3)]
        public string Category { get; set; }
        
        [LoadColumn(4)]
        public string Text { get; set; }
    }
}
