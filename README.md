# MANATOOL
Manaba Automation Tool

## これは何
筑波大学の教育支援システムであるmanabaをコマンドラインから操作するツール．  
現状では，未提出課題の一覧の確認と，出席コード提出の自動化(これは動くはずだがまだエラー処理を実装していない(というかテストができないので))のみを実装しているが，将来的には課題の提出にも対応する予定である．  

## 実装済みの機能

- 未提出課題の一覧(ただし，締切日時が設定されており，かつ現時点を基準として締切を過ぎていないこと，さらに締め切り日時が365日以内のもののみを有効な課題として表示する)を表示する : `-f`オプション，もしくは`--fetch`オプションを指定すること
- 出席コードの提出 : `-a`オプション，もしくは`--attend`オプションの後に出席コードを書くこと．ただし，この機能は動くはずだがまだテストできていないので，出席コードの提出に失敗しても何も表示されない(今後失敗した場合は祖の旨を表示する予定(学期中じゃないので出席コードを得る方法がなくてテストできない))

## 使い方

Releaseのページでバイナリを公開しているのでそれをダウンロードし，実行権限を与えて起動すればよい．  
使い方については，`-h`や`--help`オプションを与えることで確認できる．  
**また，Chromeをheadlessモードで起動し，それをSeleniumを用いて制御することによって自動化を行っているので，Chromeをインストールしておく必要がある．**(本当は僕がFirefoxユーザーなのもあってFirefoxもサポートしたかったのだが現状ではうまく動かないので，Chromeのみをサポートしている．)

## ビルド方法(自分でビルドしたい場合)

### ビルドにあたり用意するもの

* .NET Core SDK 2.2系(僕の環境は2.2.101)
* Chrome (headless modeとして使っているので)

```sh
$ git clone https://github.com/alphaKAI/manatool
$ cd manatool
$ zsh build-multi.sh
```

これで，Windows，Linux，macOS向けのバイナリのそれぞれがクロスコンパイルされ

- Windows用: `manatool-single.exe`
- Linux用: `manatool-single.linux`
- macOS用: `manatool-single.macos`

という名前で吐き出されるので，それを実行すれば良い(他のバージョンがいらない場合は適宜`build-multi.sh`を編集して他のプラットフォームについての記述を消せば良い)

## 免責事項
Manatoolはブラウザで行うことのできる操作を自動化するだけなので，特に問題はないし，[Manabaの約款](https://manaba.jp/doc/agreement/manaba_yakkan.pdf)にもスクレイピングに関する禁止は明言されていないので問題はないと思う．しかし，なにか問題が発生した場合は公開を事前の予告なく取りやめる可能性がある．

## ライセンス
ManatoolはMITライセンスのもとで配布される．

Copyright (C) 2016 Akihiro Shoji  