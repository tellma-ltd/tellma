
/**
 * Returns today on the client machine as an ISO 8601 string like this 2021-01-16T00:00:00.000.
 * The format matches how Tellma's web server formats DateTime objects into JSON.
 */
export function todayISOString(): string {
    return `${toLocalDateOnlyISOString(new Date())}T00:00:00.000`;
}

/**
 * Returns the current time on the client machine as an ISO 8601 string like this 2021-02-16T21:17:08.0723404Z
 * The format matches how Tellma's web server formats DateTimeOffset objects into JSON.
 */
export function nowISOString(): string {
    return new Date().toISOString().replace('Z', '0000Z');
}

/**
 * Returns a date object in the local time zone with the date part (year, month, day) matching the input
 * @param stringDate An ISO date representation of the form 2020-01-21T00:00:00.000
 */
export function dateFromISOString(stringDate: string): Date {
    if (!!stringDate) {
        // Extract the pieces
        const pieces = stringDate.split('T')[0].split('-');
        const year = +pieces[0];
        const month = (+pieces[1] || 1) - 1;
        const day = +pieces[2] || 1;

        // Prepare the result
        const result = new Date(year, month, day);
        result.setFullYear(year); // Avoid 30 -> 1930 conversion
        return result;
    }
}

/**
 * Returns the date part of the argument as per the local time zone, formatted as ISO 8601, for example: '2020-03-17'
 */
export function toLocalDateOnlyISOString(date: Date): string {
    // We don't rely on Date.toISOString cause it changes the date parts to UTC, causing nasty off-by-1-day bugs

    // Year
    let year = date.getFullYear().toString();
    if (year.length < 4) {
        year = '000'.substring(0, 4 - year.length) + year;
    }

    // Month
    let month = (date.getMonth() + 1).toString();
    if (month.length < 2) {
        month = '0' + month;
    }

    // Day
    let day = date.getDate().toString();
    if (day.length < 2) {
        day = '0' + day;
    }

    return `${year}-${month}-${day}`;
}

/**
 * Returns the datetime part of the argument as per the local time zone, formatted as ISO 8601, for example: '2020-03-17T11:24:13.345'
 */
export function toLocalDateTimeISOString(date: Date): string {
    // We don't rely on Date.toISOString cause it changes the date parts to UTC, causing nasty off-by-1-day bugs
    let hours = date.getHours().toString();
    if (hours.length < 2) {
        hours = '0' + hours;
    }

    let minutes = date.getMinutes().toString();
    if (minutes.length < 2) {
        minutes = '0' + minutes;
    }

    let seconds = date.getSeconds().toString();
    if (seconds.length < 2) {
        seconds = '0' + seconds;
    }

    let milliseconds = date.getMilliseconds().toString();
    if (milliseconds.length < 2) {
        milliseconds = '00'.substring(0, 3 - milliseconds.length) + milliseconds;
    }

    // This result matches how the JSON.NET-based server serializes C#'s DateTime
    return `${toLocalDateOnlyISOString(date)}T${hours}:${minutes}:${seconds}.${milliseconds}`;
}
