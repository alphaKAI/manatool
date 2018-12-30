module MyUtil

module internal String =
  let split separator (s:string) =
    let values = ResizeArray<_>()
    let rec gather start i =
      let add () = s.Substring(start,i-start) |> values.Add
      if i = s.Length then add()
      elif s.[i] = '"' then inQuotes start (i+1)
      elif s.[i] = separator then add(); gather (i+1) (i+1)
      else gather start (i+1)
    and inQuotes start i =
      if s.[i] = '"' then gather start (i+1)
      else inQuotes start (i+1)
    gather 0 0
    values.ToArray()