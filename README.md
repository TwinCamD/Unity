# TwinCam
 リアルタイム遠隔体験の映像・データ通信用システム

 Unity2017.4.XXfX LTSリリース版を利用して作成しています．

 [映像送信はこちらから](https://moritztz.github.io/twincam-video/)

 Chromeを使用して開いてください．主に映像送信と映像のみの送受信テストに使います．

# 導入

## 1

LTSリリース版のUnity2017.4.XfXをインストールする．

Unity Hubを利用すると管理が楽．
[Unity Hubをダウンロード](https://unity3d.com/jp/get-unity/download)

>Unity Hub内で
>1. インストールタブ（左）
>2. インストール（右上）
>3. Unity 2017.4.XXfX (LTS)を選択
>4. _MonoDevelop / Unity Debugger_，_Documentation_，_Standard Assets_ の3つと _Mac Build Support_ 又は _Windows Build Support_（表示されている方）の計4つにチェックをつける
>5. インストールを実行

## 2

 このページ右上の<img width="80" alt="Clone or download" src="https://user-images.githubusercontent.com/22932416/68986086-addfcb00-0867-11ea-931e-acfe3ab55be1.png">からDownload ZIPを選択して，Unityプロジェクトをダウンロードする．

## 3

ZIPファイルを展開して，Unityで開く．

## 4

`Assets/ZFBrowser/Plugins`をダウンロードする．

(GitHubの容量制限のために削除しているため)

>Unity内で
>1. Asset Storeタブを選択（右クリック → Maximize）
>2. （研究室のアカウントで）Sign inする．
>3. My Assetsから _Embedded Browser_ を検索するなりしてインポートする．
  （そのままインポートすると不足しているファイルを適当にダウンロードしてくれる）

## 5

`Assets/Main/Scenes`からシーンを選択する．

- _TwinCam_Controller_ 
  - 遠隔側（映像送信）．TwinCamのモータ制御用 主にMac側で使う．
- _TwinCam_Controller_WheelChair_
  - 車椅子用
- _TwinCam_User_Seat_
  - 体験者側（映像受信）．HMDに映像提示したりする．主にWindows側で使う．
- _TwinCam_User_WheelChair_
  - 車椅子用
- _TwinCam_Local_
  - 遠隔通信しない版．メンテナンス中．

# 使い方

## 映像受信側

1. _TwinCam_User_XXXX_ のシーンを開く
1. 