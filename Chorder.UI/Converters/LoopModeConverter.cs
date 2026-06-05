
using System.Globalization;
using System.Windows.Data;
using Chorder.Models.Entities;
namespace Chorder.UI.Converters {
public class LoopModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            LoopMode.None => "➡",   // 不循环
            LoopMode.All => "🔁",   // 列表循环
            LoopMode.One => "🔂",   // 单曲循环
            _ => "➡"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
