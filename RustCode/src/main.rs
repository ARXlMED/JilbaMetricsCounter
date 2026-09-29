fn calculate_score(values: &[i32]) -> i32 {
    let mut score = 0;
    let mut index = 0usize;

    loop {
        // CL += 1 → CL = 1;
        // loop — оператор цикла;
        // уровень 0, max = 0

        if index >= values.len() {
            // CL += 1 → CL = 2;
            // if — оператор условия;
            // уровень 1, max = 1

            break;
            // break — оператор перехода
        }

        let value = values[index];

        if value > 0 {
            // CL += 1 → CL = 3;
            // if — оператор условия;
            // уровень 1, max = 1

            score += value;
        } else if value < 0 {
            // CL += 1 → CL = 4;
            // else if — условие;
            // уровень 2, max = 2

            score -= value.abs();
        } else {
            // else не увеличивает CL

            score += 1;
        }

        index += 1;
    }

    score
}

fn classify(value: i32) -> &'static str {
    match value {
        // CL += 4 → CL = 8;
        // match сам уровень не занимает;
        // учитываются 4 обычных варианта:
        // 0, 1, 2, 3;
        // _ — default, не учитывается

        0 => "zero",   // уровень 0, max = 2
        1 => "one",    // уровень 1, max = 2
        2 => "two",    // уровень 2, max = 2
        3 => "three",  // уровень 3, max = 3
        _ => "other",  // default, не учитывается
    }
}

fn process(values: &[i32]) -> i32 {
    let mut total = 0;

    for value in values {
        // CL += 1 → CL = 9;
        // for — оператор цикла;
        // уровень 0, max = 3

        if value % 2 == 0 {
            // CL += 1 → CL = 10;
            // if — оператор условия;
            // уровень 1, max = 3

            total += value;
        }
    }

    let mut counter = 0usize;

    while counter < values.len() {
        // CL += 1 → CL = 11;
        // while — оператор цикла;
        // уровень 0, max = 3

        total += values[counter];
        counter += 1;
    }

    let mut position = 0usize;

    while let Some(value) = values.get(position) {
        // CL += 1 → CL = 12;
        // while — оператор цикла;
        // уровень 0, max = 3

        if *value > 10 {
            // CL += 1 → CL = 13;
            // if — оператор условия;
            // уровень 1, max = 3

            total += 2;
        }

        position += 1;
    }

    let mut number = 0;

    loop {
        // CL += 1 → CL = 14;
        // loop — оператор цикла;
        // уровень 0, max = 3

        number += 1;

        if number > 3 {
            // CL += 1 → CL = 15;
            // if — оператор условия;
            // уровень 1, max = 3

            break;
            // break — оператор перехода
        }

        for _ in 0..1 {
            // CL += 1 → CL = 16;
            // for — оператор цикла;
            // уровень 1, max = 3

            match number {
                // CL += 3 → CL = 19;
                // match сам уровень не занимает;
                // учитываются 3 обычных варианта:
                // 1, 2, 3;
                // _ — default, не учитывается

                1 => {
                    // уровень 2, max = 3

                    match number {
                        // CL += 1 → CL = 20;
                        // match сам уровень не занимает;
                        // учитывается 1 обычный вариант: 1;
                        // _ — default, не учитывается

                        1 => {
                            // уровень 3, max = 3

                            if total > 0 {
                                // CL += 1 → CL = 21;
                                // if — оператор условия;
                                // уровень 4, max = 4

                                total += 1;
                            }
                        }
                        _ => {}
                    }
                }

                2 => total += 2,
                // уровень 3, max = 4

                3 => total += 3,
                // уровень 4, max = 4
                _ => total += 0,
                // default, не учитывается
            }
        }
    }

    total
}

fn main() {
    let values = vec![0, 1, 2, 5, 10, 15];

    let score = calculate_score(&values);
    let result = process(&values);

    println!("Score: {}", score);
    println!("Result: {}", result);

    for value in &values {
        // CL += 1 → CL = 22;
        // for — оператор цикла;
        // уровень 0, max = 4

        println!("{} -> {}", value, classify(*value));
    }
}