module Manaba
open System
open System.IO
open System.Reflection
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Firefox
open OpenQA.Selenium
open FSharp.Scanf
open MyUtil

let manaba_url = "http://manaba.tsukuba.ac.jp/"
let attend_url = "https://atmnb.tsukuba.ac.jp/attend/tsukuba?lang=ja"

type Browser =
  | Firefox of FirefoxDriver
  | Chrome of ChromeDriver

  static member new_firefox () =
    let options = new FirefoxOptions()
    options.AddArgument("-headless")
    Firefox(new FirefoxDriver(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), options))

  static member new_chrome () =
    let options = new ChromeOptions()
    options.AddArgument("--headless")
    Chrome (new ChromeDriver(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), options))

  member this.Url
    with get() =
      match this with
      | Firefox firefox -> firefox.Url
      | Chrome chrome -> chrome.Url
    and set(value) =
      match this with
      | Firefox firefox -> firefox.Url <- value
      | Chrome chrome -> chrome.Url <- value

  member this.FindElementByXPath =
    match this with
      | Firefox firefox -> firefox.FindElementByXPath
      | Chrome chrome -> chrome.FindElementByXPath

  member this.Title =
    match this with
      | Firefox firefox -> firefox.Title
      | Chrome chrome -> chrome.Title

  member this.Quit =
    match this with
      | Firefox firefox -> firefox.Quit
      | Chrome chrome -> chrome.Quit


type Report = {
  class_name: string
  title: string
  begin_date: DateTime
  end_date: DateTime option
}

let inline print_report (report: Report) =
  printfn "[%s] %s [%A - %A]" report.class_name report.title report.begin_date report.end_date

exception InvalidCredentialsException

type Manaba(id: string, pw: string) =
  let browser = Browser.new_chrome ()
  do
    browser.Url <- manaba_url
    let id_form = browser.FindElementByXPath("//*[@id=\"username\"]")
    id_form.SendKeys(id)
    let pw_form = browser.FindElementByXPath("//*[@id=\"password\"]")
    pw_form.SendKeys(pw)
    pw_form.SendKeys(Keys.Enter)
    if browser.Title <> "manaba - home" then
      stderr.WriteLine "Login failed,  ID or PASSWORD is probably incorrect."
      stderr.WriteLine "Delete setting.bin, and please retry."
      raise InvalidCredentialsException
    else
      printfn "Success to Login"

  member this.terminate () =
    // Console.ReadKey () |> ignore
    browser.Quit ()

  (*
    出席の自動化
    引数は出席コード
  *)
  member this.attend code =
    browser.Url <- attend_url
    let code_form = browser.FindElementByXPath("/html/body/div/div[2]/div[1]/form/div/div/div/table/tbody/tr[1]/td/input")
    code_form.SendKeys(code)
    code_form.SendKeys(Keys.Enter)
    // TODO: 出席コードが正しいか確認する必要がある．
    browser.Url <- manaba_url

  // 未提出レポートの一覧を得る．
  member this.getReports () =
    let reports_url = "https://manaba.tsukuba.ac.jp/ct/home_library_query"
    browser.Url <- reports_url
    let tbody = browser.FindElementByXPath("/html/body/div[2]/div[2]/div[2]/table/tbody")
    let row0 = tbody.FindElements(By.ClassName("row0"))
    let row1 = tbody.FindElements(By.ClassName("row1"))

    let parse_report text =
      let splitted = String.split '\n' text
      let (begin_date, end_date) =
        trySscanf "%d-%d-%d %d:%d %d-%d-%d %d:%d" splitted.[3]
        |> function
          | Ok (y1, m1, d1, h1, mm1, y2, m2, d2, h2, mm2) ->
            (DateTime(y1, m1, d1, h1, mm1, 0), Some (DateTime(y2, m2, d2, h2, mm2, 0)))
          | Error _ ->
            let (y1, m1, d1, h1, mm1) = sscanf "%d-%d-%d %d:%d" splitted.[3]
            (DateTime(y1, m1, d1, h1, mm1, 0), None)
      {
        class_name = splitted.[1]
        title = splitted.[2]
        begin_date = begin_date
        end_date = end_date
      }

    let reports = Set.union (Set.ofSeq (seq { for x in row0 do yield parse_report x.Text }))
                            (Set.ofSeq (seq { for x in row1 do yield parse_report x.Text }))

    browser.Url <- manaba_url
    reports

    // 有効(現時点を基準に，提出期限内かつ，提出期限が365日以内のもの)なレポート一覧を返す
    member this.activeReports () =
      this.getReports ()
      |> Set.filter (fun x ->
        let now = DateTime.Now
        let l = x.begin_date <= now
        match x.end_date with
        | Some ed ->
          // 提出期限が365日以上先の課題は常識的に考えて除外して良い
          if ed <= now.Add (TimeSpan.FromDays 365.) then
            l && now <= ed
          else
            false
          // 終了期限が設定されてないのも除外
        | None -> false)
