using Microsoft.ML.Data;

namespace Vk.Post.Predict.Models
{
    public class VkMessagePredict
    {
        [ColumnName("PredictedLabel")]
        public string Category;

        public float[] Score;
    }
}
