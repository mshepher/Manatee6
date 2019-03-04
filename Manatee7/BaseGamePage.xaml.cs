using Log = Serilog.Log;
using Manatee5.Model;

using Xamarin.Forms;
// ReSharper disable InconsistentNaming -- protected classes should count as private


namespace Manatee5 {
  public abstract partial class BaseGamePage : ContentPage {

    protected static readonly Game game = Game.Instance;
    protected static readonly GameController controller = GameController.Instance;

    protected BaseGamePage() {
      InitializeComponent();
    }

    protected override bool OnBackButtonPressed() {
      return true;
    }
  }
}