using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginWallet : MonoBehaviour
{
    
    /*[SerializeField]
    private Button loginBtn;
    
    [SerializeField]
    private Button loginBtnTwitter;
    
    [SerializeField]
    private Button loginBtnSms;
    [SerializeField]
    private Button loginBtnXNFT;*/
    [SerializeField]
    private TextMeshProUGUI messageTxt;
   // [SerializeField]
    //private Button loginBtnGoogle;
    [SerializeField]
    private Button loginBtnWalletAdapter;
    private void OnEnable()
    {
       

        if (Web3.Wallet != null)
        {
           
            gameObject.SetActive(false);
        }
    }

    private void Start()
    {


       /* loginBtn.onClick.AddListener(LoginChecker);
        loginBtnTwitter.onClick.AddListener(delegate { LoginCheckerWeb3Auth(Provider.TWITTER); });
        loginBtnSms.onClick.AddListener(LoginCheckerSms);
        loginBtnXNFT.onClick.AddListener(LoginCheckerWalletAdapter);

        loginBtnXNFT.gameObject.SetActive(false);*/

       // loginBtnGoogle.onClick.AddListener(delegate { LoginCheckerWeb3Auth(Provider.GOOGLE); });
        loginBtnWalletAdapter.onClick.AddListener(LoginCheckerWalletAdapter);
        if (Application.platform is RuntimePlatform.LinuxEditor or RuntimePlatform.WindowsEditor or RuntimePlatform.OSXEditor)
        {
            loginBtnWalletAdapter.onClick.RemoveListener(LoginCheckerWalletAdapter);
            loginBtnWalletAdapter.onClick.AddListener(() =>
                Debug.LogWarning("Wallet adapter login is not yet supported in the editor"));
        }

        if (messageTxt != null)
            messageTxt.gameObject.SetActive(false);
    }
    private async void LoginChecker()
    {
        
        //var account = await Web3.Instance.LoginInGameWallet(password);
        //CheckAccount(account);
    }

    private async void LoginCheckerSms()
    {
        var account = await Web3.Instance.LoginWalletAdapter();
        CheckAccount(account);
    }

    private async void LoginCheckerWeb3Auth(Provider provider)
    {
        var account = await Web3.Instance.LoginWeb3Auth(provider);
        CheckAccount(account);
    }

    private async void LoginCheckerWalletAdapter()
    {
        if (Web3.Instance == null) return;
        var account = await Web3.Instance.LoginWalletAdapter();
        messageTxt.text = "";
        CheckAccount(account);
    }


    private void CheckAccount(Account account)
    {
        if (account != null)
        {
            
            messageTxt.gameObject.SetActive(false);
            //gameObject.SetActive(false);
            SceneManager.LoadScene("Main Menu");
        }
        else
        {
            messageTxt.gameObject.SetActive(true);
        }
    }

    public void OnClose()
    {
        var wallet = GameObject.Find("wallet");
        wallet.SetActive(false);
    }
}

