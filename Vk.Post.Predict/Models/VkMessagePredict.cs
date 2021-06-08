using Microsoft.ML.Data;

namespace Vk.Post.Predict
{
    public class VkMessagePredict
    {
        [ColumnName("PredictedLabel")]
        public string Category;

        public float[] Score;
    }
}