＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠
＠＠＠                                                                  ＠＠＠
＠＠＠      ゆかりすたー METEOR                                         ＠＠＠
＠＠＠                      ソースコードについて        2019/06/29      ＠＠＠
＠＠＠                                                                  ＠＠＠
＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠＠


===============================================================================
≪   名    称   ≫  ゆかりすたー METEOR
≪  バージョン  ≫  Ver 2.02 α
≪   作    者   ≫  SHINTA
≪ ホームページ ≫  http://shinta.coresv.com/software/yukalister_jpn/
≪  E メ ー ル  ≫  shinta.0806 <at> gmail.com
===============================================================================


【　目次　】------------------------------------------------

・内容概要
・ビルド方法
・規約


【　内容概要　】--------------------------------------------

　ゆかりすたーのソースコード一式です。

　開発環境は、Visual Studio Community 2017 です。

　なお、ソースコードに関するサポートはできかねますのでご了承下さい。


【　ビルド方法　】------------------------------------------

　ゆかりすたーのビルド・実行には、本プロジェクトの他に、以下が必要です。

・SHINTA 共通 C# ライブラリー
    https://github.com/shinta0806/CommonCsLib
・Livet
    https://github.com/ugaya40/Livet
・MaterialDesignInXamlToolkit
    https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit
・System.Data.SQLite
    https://system.data.sqlite.org/index.html/doc/trunk/www/index.wiki
・ReadJEnc
    https://github.com/hnx8/ReadJEnc

　それぞれのファイル群を、例えば以下のように配置してください。ゆかりすたーのファイル群よりも、SHINTA 共通 C# ライブラリーのファイル群が 1 階層上位になるように配置します。

C:\
   CsProjects\
              _Common\           ← SHINTA 共通 C# ライブラリーファイル群
              YukaLister\
                        Src\     ← ゆかりすたーファイル群
                                 ← Livet.Extensions.dll
                                 ← Microsoft.WindowsAPICodePack.dll
                                 ← Microsoft.WindowsAPICodePack.Shell.dll
                                 ← MaterialDesignColors.dll
                                 ← MaterialDesignThemes.Wpf.dll
                                 ← System.Data.SQLite.dll
                                 ← System.Data.SQLite.Linq.dll
                                 ← Hnx8.ReadJEnc.dll
                           x86\  ← SQLite.Interop.dll


【　規約　】------------------------------------------------

　ゆかりすたーのソースコードは、
クリエイティブ・コモンズ・ライセンス（表示 2.1 日本）
https://creativecommons.org/licenses/by/2.1/jp/
の下に公開されています。


