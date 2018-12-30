module Program
open Manaba
open System
open FSharp.CommandLine
open FSharp.CommandLine.Options
open FSharp.CommandLine.Commands
open System.IO
open MBrace.FsPickler

type Setting = {
  id: string
  pw: string
}

let inline writeSetting file setting =
  let binarySerializer = FsPickler.CreateBinarySerializer()
  let pickle = binarySerializer.Pickle setting
  File.WriteAllBytes (file, pickle)

let inline readSetting file =
  if File.Exists file then
    let binarySerializer = FsPickler.CreateBinarySerializer()
    Some (File.ReadAllBytes file |> binarySerializer.UnPickle<Setting>)
  else
    None

let makeSetting () =
  printfn "s17xxxxxのIDとパスワードを入力してください"
  printf "ID: "
  let id = Console.ReadLine ()
  printfn ""
  printf "PW: "
  let pw = Console.ReadLine ()
  printfn ""
  {id = id; pw = pw}


let attendOption =
  commandOption {
    names ["a"; "attend"]
    description "引数に与えられた出席コードを提出し，授業に出席する．"
    takes (format("%s").map (fun code -> code))
  }

let fetchOption =
  commandFlag {
    names ["f"; "fetch"]
    description "未提出レポートの一覧を表示する．(提出締切期限を過ぎていない，かつ締切日時が365日以上のものは表示されない)"
  }

let mainCommand () =
  command {
    name "manatool"
    description "Manaba Automation Tool"
    opt code in attendOption |> CommandOption.zeroOrExactlyOne
    opt fetch in fetchOption |> CommandOption.zeroOrExactlyOne
                             |> CommandOption.whenMissingUse false
    do
      if (match code with | None -> false | _ -> true) || fetch then
        let file_path = "setting.bin"
        let setting =
          readSetting file_path
          |> function
          | Some setting -> setting
          | None ->
            let ret = makeSetting ()
            writeSetting file_path ret
            ret

        printfn "ID: %A" setting.id

        let manaba = new Manaba(setting.id, setting.pw)

        match code with
        | Some code -> manaba.attend code
        | None _ -> ()

        if fetch then
          manaba.activeReports ()
          |> Seq.iter print_report

        manaba.terminate ()
      else
        printfn "オプションが指定されていません．"
        printfn "オプションについては -h オプションでヘルプを見て確認してください．"
    return 0
  }

[<EntryPoint>]
let main argv =
  mainCommand () |> Command.runAsEntryPoint argv