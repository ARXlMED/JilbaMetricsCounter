using System;
using System.Collections.Generic;
using System.Text;

namespace JilbaMetricsCounter
{
    public static class GilbAnalyzer
    {
        public class Result
        {
            public int CL { get; set; }
            public int CLI { get; set; }
            public int N { get; set; }
            public double cl => N > 0 ? (double)CL / N : 0;
            public int McCabe => CL + 1;

            public List<string> Trace { get; } = new();
        }

        enum K { Id, Num, Str, Sym }
        class Tk
        {
            public K Kind; public string Value;
            public Tk(K k, string v) { Kind = k; Value = v; }
        }

        // ============== Токенайзер ==============
        static List<Tk> Tokenize(string s)
        {
            var L = new List<Tk>();
            int i = 0, n = s.Length;
            while (i < n)
            {
                char c = s[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }
                if (c == '/' && i + 1 < n && s[i + 1] == '/')
                { while (i < n && s[i] != '\n') i++; continue; }
                if (c == '/' && i + 1 < n && s[i + 1] == '*')
                { i += 2; while (i + 1 < n && !(s[i] == '*' && s[i + 1] == '/')) i++; i += 2; continue; }
                if (c == '"')
                {
                    int st = i; i++;
                    while (i < n && s[i] != '"')
                    { if (s[i] == '\\' && i + 1 < n) i++; i++; }
                    i++;
                    L.Add(new Tk(K.Str, s.Substring(st, i - st)));
                    continue;
                }
                if (char.IsDigit(c))
                {
                    int st = i;
                    while (i < n && (char.IsDigit(s[i]) || s[i] == '_' ||
                           (s[i] == '.' && i + 1 < n && char.IsDigit(s[i + 1])))) i++;
                    L.Add(new Tk(K.Num, s.Substring(st, i - st)));
                    continue;
                }
                if (char.IsLetter(c) || c == '_')
                {
                    int st = i;
                    while (i < n && (char.IsLetterOrDigit(s[i]) || s[i] == '_')) i++;
                    L.Add(new Tk(K.Id, s.Substring(st, i - st)));
                    continue;
                }
                string[] multi = {
                    "<<=", ">>=", "..=", "->", "=>", ">=", "<=", "==", "!=",
                    "&&", "||", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=",
                    "<<", ">>", "..", "::"
                };
                bool m = false;
                foreach (var op in multi)
                    if (i + op.Length <= n && s.Substring(i, op.Length) == op)
                    { L.Add(new Tk(K.Sym, op)); i += op.Length; m = true; break; }
                if (m) continue;
                L.Add(new Tk(K.Sym, c.ToString()));
                i++;
            }
            return L;
        }

        // ============== Парсер ==============
        static List<Tk> T;
        static int pos;
        static Result R;
        static int level;
        static int maxLevel;

        public static Result Analyze(string code)
        {
            T = Tokenize(code);
            pos = 0;
            level = 0;
            maxLevel = 0;
            R = new Result();

            ParseBlock(0);
            R.CLI = maxLevel;
            R.N = CountN(T);   // ← пересчитываем N отдельно

            return R;
        }

        static void ParseBlock(int lvl)
        {
            while (pos < T.Count && T[pos].Value != "}")
            {
                var t = T[pos];

                if (t.Value == "fn")
                {
                    // пропускаем сигнатуру до {
                    pos++;
                    if (pos < T.Count && T[pos].Kind == K.Id) pos++;  // имя
                    while (pos < T.Count && T[pos].Value != "{") pos++;
                    if (pos < T.Count && T[pos].Value == "{")
                    {
                        pos++;
                        ParseBlock(lvl);   // тело функции на том же уровне
                    }
                    continue;
                }

                if (t.Value == "if") { ParseIf(lvl); continue; }
                if (t.Value == "while") { ParseWhile(lvl); continue; }
                if (t.Value == "for") { ParseFor(lvl); continue; }
                if (t.Value == "loop") { ParseLoop(lvl); continue; }
                if (t.Value == "match") { ParseMatch(lvl); continue; }

                // прочие операторы для N
                if (t.Value == "let" ||
                    t.Value == "return" || t.Value == "break" || t.Value == "continue" ||
                    t.Value == "=" || t.Value == "+=" || t.Value == "-=" ||
                    t.Value == "*=" || t.Value == "/=" || t.Value == "%=")
                {
                    R.N++;
                }
                pos++;
            }
            if (pos < T.Count && T[pos].Value == "}") pos++;
        }

        static void ParseIf(int lvl)
        {
            // Этот if
            R.CL++;
            R.Trace.Add($"if на уровне {lvl}: CL = {R.CL}");
            if (lvl > maxLevel) maxLevel = lvl;

            pos++;  // if
            // пропускаем условие до {
            while (pos < T.Count && T[pos].Value != "{") pos++;
            if (pos < T.Count && T[pos].Value == "{")
            {
                pos++;
                ParseBlock(lvl + 1);
            }

            // else / else if
            while (pos < T.Count && T[pos].Value == "else")
            {
                pos++;
                if (pos < T.Count && T[pos].Value == "if")
                {
                    // else if — уровень + 1
                    int elifLevel = lvl + 1;
                    R.CL++;
                    R.Trace.Add($"else if на уровне {elifLevel}: CL = {R.CL}");
                    if (elifLevel > maxLevel) maxLevel = elifLevel;

                    pos++;  // if
                    while (pos < T.Count && T[pos].Value != "{") pos++;
                    if (pos < T.Count && T[pos].Value == "{")
                    {
                        pos++;
                        ParseBlock(elifLevel + 1);
                    }
                    // следующая итерация цикла if словит ещё else, если есть
                }
                else if (pos < T.Count && T[pos].Value == "{")
                {
                    pos++;
                    ParseBlock(lvl + 1);
                    break;
                }
                else break;
            }
        }

        static void ParseWhile(int lvl)
        {
            R.CL++;
            R.Trace.Add($"while на уровне {lvl}: CL = {R.CL}");
            if (lvl > maxLevel) maxLevel = lvl;
            R.N++;

            pos++;
            // если while let — пропускаем let
            if (pos < T.Count && T[pos].Value == "let") pos++;
            while (pos < T.Count && T[pos].Value != "{") pos++;
            if (pos < T.Count && T[pos].Value == "{")
            {
                pos++;
                ParseBlock(lvl + 1);
            }
        }

        static void ParseFor(int lvl)
        {
            R.CL++;
            R.Trace.Add($"for на уровне {lvl}: CL = {R.CL}");
            if (lvl > maxLevel) maxLevel = lvl;
            R.N++;

            pos++;
            while (pos < T.Count && T[pos].Value != "{") pos++;
            if (pos < T.Count && T[pos].Value == "{")
            {
                pos++;
                ParseBlock(lvl + 1);
            }
        }

        static void ParseLoop(int lvl)
        {
            R.CL++;
            R.Trace.Add($"loop на уровне {lvl}: CL = {R.CL}");
            if (lvl > maxLevel) maxLevel = lvl;
            R.N++;

            pos++;
            while (pos < T.Count && T[pos].Value != "{") pos++;
            if (pos < T.Count && T[pos].Value == "{")
            {
                pos++;
                ParseBlock(lvl + 1);
            }
        }

        static void ParseMatch(int lvl)
        {
            pos++;  // match

            // пропускаем выражение-селектор до {
            while (pos < T.Count && T[pos].Value != "{") pos++;
            if (pos >= T.Count) return;
            pos++;  // {

            int armIndex = 0;
            while (pos < T.Count && T[pos].Value != "}")
            {
                // Начало ветки: идём до => 
                int startArm = pos;
                while (pos < T.Count && T[pos].Value != "=>" && T[pos].Value != "}") pos++;
                if (pos >= T.Count || T[pos].Value == "}") break;

                // Смотрим, что было до =>
                bool isDefault = false;
                for (int k = startArm; k < pos; k++)
                    if (T[k].Value == "_") { isDefault = true; break; }

                pos++;  // =>

                int thisArmLevel = lvl + armIndex;
                if (!isDefault)
                {
                    R.CL++;
                    R.Trace.Add($"match-ветка #{armIndex} на уровне {thisArmLevel}: CL = {R.CL}");
                    if (thisArmLevel > maxLevel) maxLevel = thisArmLevel;

                    // тело ветки на уровне +1
                    if (pos < T.Count && T[pos].Value == "{")
                    {
                        pos++;
                        ParseBlock(thisArmLevel + 1);
                    }
                    else
                    {
                        // выражение до ,
                        while (pos < T.Count && T[pos].Value != "," && T[pos].Value != "}") pos++;
                        if (pos < T.Count && T[pos].Value == ",") pos++;
                    }
                }
                else
                {
                    // default — пропускаем тело
                    if (pos < T.Count && T[pos].Value == "{")
                    {
                        int depth = 1; pos++;
                        while (pos < T.Count && depth > 0)
                        {
                            if (T[pos].Value == "{") depth++;
                            else if (T[pos].Value == "}") depth--;
                            pos++;
                        }
                    }
                    else
                    {
                        while (pos < T.Count && T[pos].Value != "," && T[pos].Value != "}") pos++;
                        if (pos < T.Count && T[pos].Value == ",") pos++;
                    }
                }

                armIndex++;
            }

            if (pos < T.Count && T[pos].Value == "}") pos++;
        }


        static readonly HashSet<string> BuiltinConstructors = new()
{
    "Some", "None", "Ok", "Err"
};

        // Линейный подсчёт N по правилам:
        //   let ...                → 1
        //   if / for / while / loop / match → 1
        //   return / break / continue → 1
        //   += / -= / *= / /= / %=  → 1
        //   вызовы f() / x.y() / f!() → 1
        //   неявный возврат (последний токен тела fn — идентификатор) → 1
        static int CountN(List<Tk> T)
        {
            int count = 0;
            int i = 0;
            while (i < T.Count)
            {
                var t = T[i];

                // ---------- функция ----------
                if (t.Value == "fn")
                {
                    int j = i + 1;
                    if (j < T.Count && T[j].Kind == K.Id) j++;       // имя
                    while (j < T.Count && T[j].Value != "{") j++;    // до {
                    if (j >= T.Count) { i = j; continue; }
                    j++;                                             // за {
                    int bodyStart = j;

                    int depth = 1;
                    while (j < T.Count && depth > 0)
                    {
                        if (T[j].Value == "{") depth++;
                        else if (T[j].Value == "}") { depth--; if (depth == 0) break; }
                        j++;
                    }
                    // j — на закрывающей } функции
                    if (j > 0 && T[j - 1].Kind == K.Id) count++;     // неявный return

                    i = bodyStart;
                    continue;
                }

                // ---------- let ----------
                if (t.Value == "let") { count++; i++; continue; }

                // ---------- while (в т.ч. while let) ----------
                if (t.Value == "while")
                {
                    count++;
                    i++;
                    if (i < T.Count && T[i].Value == "let") i++;    // часть while let
                    continue;
                }

                // ---------- if / for / loop / match ----------
                if (t.Value == "if" || t.Value == "for" ||
                    t.Value == "loop" || t.Value == "match")
                {
                    count++;
                    i++;
                    if (t.Value == "if" && i < T.Count && T[i].Value == "let") i++;
                    continue;
                }

                // ---------- return / break / continue ----------
                if (t.Value == "return" || t.Value == "break" || t.Value == "continue")
                {
                    count++; i++; continue;
                }

                // ---------- составные присваивания ----------
                if (t.Value == "+=" || t.Value == "-=" || t.Value == "*=" ||
                    t.Value == "/=" || t.Value == "%=")
                {
                    count++; i++; continue;
                }

                // ---------- вызовы ----------
                if (t.Kind == K.Id && !BuiltinConstructors.Contains(t.Value))
                {
                    // f!( ... )
                    if (i + 2 < T.Count && T[i + 1].Value == "!" && T[i + 2].Value == "(")
                    { count++; i += 3; continue; }

                    // f![ ... ]
                    if (i + 2 < T.Count && T[i + 1].Value == "!" && T[i + 2].Value == "[")
                    { count++; i += 3; continue; }


                    // f( ... )
                    if (i + 1 < T.Count && T[i + 1].Value == "(")
                    { count++; i += 2; continue; }

                    // x.y( ... )
                    if (i + 3 < T.Count && T[i + 1].Value == "." &&
                        T[i + 2].Kind == K.Id && T[i + 3].Value == "(")
                    { count++; i += 4; continue; }
                }

                i++;
            }
            return count;
        }
    }
}
