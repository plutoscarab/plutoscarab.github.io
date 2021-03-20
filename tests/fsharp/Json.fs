namespace fsharp

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open PlutoScarab.Parser
open PlutoScarab.Json

[<TestClass>]
type JsonTests () =

    let randomNumber (rand : Random) =
        let x = Math.Round(rand.NextDouble() - 0.5, 4)
        let y = Math.Pow(10.0, float (rand.Next(50)) - 25.0)
        x * y

    let randomJNumber (rand : Random) =
        JNumber (randomNumber rand)

    let randomBadNumber (rand : Random) =
        let s = (randomNumber rand).ToString()
        match rand.Next(50) with
        | 0 ->
            "0" + s
        | _ ->
            let i = rand.Next(s.Length + 1)
            s.Substring(0, i) + "x" + s.Substring(i)

    let randomChar (rand : Random) = 

        let cp = 
            match rand.Next(10) with
            // surrogate pair required
            | 0 -> rand.Next(0x10000, 0x110000)

            // greater than surrogate range
            | 1 -> rand.Next(0xE000, 0x10000)

            // control character (which would otherwise be rare)
            | 2 -> rand.Next(0x20)

            // less than surrogate range
            | _ -> rand.Next(0xD800)

        Char.ConvertFromUtf32(cp)

    let randomString (rand : Random) =
        [1..rand.Next(20)] |> Seq.map (fun _ -> randomChar rand) |> String.Concat

    let randomJString (rand : Random) =
        JString (randomString rand)

    let decode (s : string) =
        s 
        |> Seq.map 
            (fun ch -> 
                let code = $"{(int ch):X4}".TrimStart('0')
                code + $"({ch}) ")
        |> String.Concat
    
    let randomBadString (rand : Random) =
        let ch =
            match rand.Next(2) with
            // Control character
            | 0 -> string (char (rand.Next(32))) 

            // Naked surrogate
            | _ -> string (char (rand.Next(0xD800, 0xE000)))

        let s = toStr (randomJString rand)
        let i = rand.Next(1, s.Length) // inside the surrounding quotes
        s.Substring(0, i) + ch + s.Substring(i)

    let randomJSimple (rand : Random) =
        match rand.Next(3) with
        | 0 -> JNull
        | 1 -> JBool false
        | _ -> JBool true

    let rec randomJArray (rand : Random) =
        JArray ([1..rand.Next(7)] |> Seq.map (fun _ -> randomJValue rand) |> Seq.toList)

    and randomJObject (rand : Random) =

        let randomJMember (rand : Random) =
            (randomString rand, randomJValue rand)

        JObject ([1..rand.Next(7)] |> Seq.map (fun _ -> randomJMember rand) |> Seq.toList)

    and randomJValue (rand : Random) =
        match rand.Next(7) with
        | 0 -> randomJArray rand
        | 1 -> randomJObject rand
        | 2 -> randomJSimple rand
        | 3 -> randomJNumber rand
        | _ -> randomJString rand
    
    let deny s r =
        match r with
        | Err(_, _) -> ()
        | Fatal(_, _) -> ()
        | Ok(_, _, c) when c.index <> c.text.Length -> () 
        | Ok(text, _, _) -> Assert.Fail("\nmatched:  " + decode(text) + "\noriginal: " + decode(s))

    [<TestMethod>]
    member this.JNullOk () =
        let c = { text = "null"; index = 0 }
        let r = jnull c
        match r with
        | Err(msg, i) -> Assert.Fail(msg)
        | _ -> Assert.IsTrue(true)

    [<TestMethod>]
    member this.JNullErr () =
        let c = { text = "nule"; index = 0 }
        let r = jnull c
        match r with
        | Ok(text, _, _) -> Assert.Fail($"found {text}")
        | _ -> Assert.IsTrue(true)

    [<TestMethod>]
    member this.JNumberOk () =
        let rand = new Random()

        for _ in [1..1000] do
            let j = randomJNumber rand
            let s = toStr j
            let r = jnumber { text = s; index = 0 }
            match r with
            | Ok(_, [value], _) -> Assert.AreEqual(j, value)
            | _ -> Assert.Fail(string r)

    [<TestMethod>]
    member this.JNumberErr () =
        let rand = new Random()

        for _ in [1..1000] do
            let s = randomBadNumber rand
            let r = jnumber { text = s; index = 0 }
            deny s r

    [<TestMethod>]
    member this.JStringOk () =
        let rand = new Random()

        for _ in [1..1000] do
            let j = randomJString rand
            let s = toStr j
            let r = jstring { text = s; index = 0 }
            match r with
            | Ok(_, [value], _) -> Assert.AreEqual(j, value)
            | _ -> Assert.Fail(string r)

    [<TestMethod>]
    member this.JStringErr () =
        let rand = new Random()

        for _ in [1..1000] do
            let s = randomBadString rand
            let r = jstring { text = s; index = 0 }
            deny s r

    [<TestMethod>]
    member this.JValueOk () =
         let rand = new Random()

         for _ in [1..1000] do
            let j = randomJValue rand
            let s = toIndented j
            let r = jparser s
            match r with
            | Ok(_, [value], _) -> Assert.AreEqual(j, value)
            | _ -> Assert.Fail(string r)

