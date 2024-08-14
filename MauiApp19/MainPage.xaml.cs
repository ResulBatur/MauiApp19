using Services.MauiApp19.Helpers;
namespace MauiApp19
{
    public partial class MainPage : ContentPage
    {
        private readonly GoogleAuthHelper _googleAuthHelper;
        public MainPage()
        {
            InitializeComponent();
            _googleAuthHelper = new GoogleAuthHelper(
              "218858434869-idj0lukpqiiq89s0qmv3gml6ooeercvq.apps.googleusercontent.com",
              "",
              "https://localhost/oauth2redirect");
        }
        private async void OnCounterClicked(object sender, EventArgs e)
        {
            try
            {
                var (idToken, scope) = await _googleAuthHelper.Authenticate();
                var userInfo = GoogleAuthHelper.GetUserInfoFromToken(idToken);
                lbl.Text = $"Ad: {userInfo.Name}\nE-posta: {userInfo.Email}\nKapsam: {scope}";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

    }
}