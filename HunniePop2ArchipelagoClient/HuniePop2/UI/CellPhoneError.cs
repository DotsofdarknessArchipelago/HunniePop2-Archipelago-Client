using System.Threading.Tasks;

namespace HunniePop2ArchipelagoClient.HuniePop2.UI
{
    public class CellPhoneError
    {

        /// <summary>
        /// popup an error box on the screen
        /// </summary>
        public static async void cellerror(string s, UiCellphone c)
        {
            c.phoneErrorMsg.ShowMessage(s);
            await Task.Delay(5000);
            c.phoneErrorMsg.ClearMessage();
        }
    }
}
