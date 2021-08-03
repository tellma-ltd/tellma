/**
 * The Levenshtein distance between two strings, with a maximum cap for optimization (smallest cap is 1).
 * The code is modified from https://bit.ly/2TiTyWZ, credit to Andrei Mackenzie
 */
export function getEditDistance(a: string, b: string, cap: number = 10000000000): number {
    a = a || '';
    b = b || '';

    // Optimizations
    if (a === b) {
        return 0;
    } else if (cap <= 1) {
        return 1;
    } else if (a.length === 0) {
        return Math.min(cap, b.length);
    } else if (b.length === 0) {
        return Math.min(cap, a.length);
    }

    const matrix = [];

    // Reused varaiables
    let capped = true;
    let distance: number;
    let substitutionCost: number;

    // Increment along the first column of each row
    let i = 0;
    for (; i <= b.length; i++) {
        matrix[i] = [i];
    }

    // Increment each column in the first row
    let j = 0;
    for (; j <= a.length; j++) {
        matrix[0][j] = j;
    }

    // Fill in the rest of the matrix
    for (i = 1; i <= b.length; i++) {
        capped = true;
        for (j = 1; j <= a.length; j++) {
            if (b.charAt(i - 1) === a.charAt(j - 1)) {
                substitutionCost = 0;
            } else {
                substitutionCost = 1;
            }

            distance = Math.min(
                matrix[i - 1][j] + 1, // deletion
                matrix[i][j - 1] + 1, // insertion
                matrix[i - 1][j - 1] + substitutionCost); // substitution

            matrix[i][j] = distance;

            // Optimization: If the entire row is >= cap, then we terminate the outer loop
            if (capped && distance < cap) {
                capped = false;
            }
        }

        if (capped) {
            return cap;
        }
    }

    return matrix[b.length][a.length];
}
