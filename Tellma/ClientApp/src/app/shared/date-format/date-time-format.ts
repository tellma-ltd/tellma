import { TranslateService } from '@ngx-translate/core';
import {
    adjustDateFormatForGranularity,
    adjustTimeFormatForGranularity,
    formatDate,
    formatTime,
    ngbDateFromDate,
    ngbTimeFromDate
} from '~/app/data/date-time-formats';
import { Calendar, DateGranularity, DateTimeGranularity, TimeGranularity } from '~/app/data/entities/base/metadata-types';
import { WorkspaceService } from '~/app/data/workspace.service';

export function dateFormat(
    isoDate: string,
    wss: WorkspaceService,
    trx: TranslateService,
    calendar?: Calendar,
    granularity?: DateGranularity) {

    if (!!isoDate) {
        const jsDate = new Date(isoDate);
        calendar = calendar || wss.calendar;
        const format = adjustDateFormatForGranularity(wss.dateFormat, granularity);
        const ngbDate = ngbDateFromDate(jsDate, calendar);
        return formatDate(ngbDate, format, trx, calendar);
    } else {
        return '';
    }
}

export function timeFormat(
    isoDate: string,
    wss: WorkspaceService,
    trx: TranslateService,
    granularity?: TimeGranularity) {

    if (!!isoDate) {
        const jsDate = new Date(isoDate);
        if (isNaN(jsDate.getTime())) {
            return '';
        }

        // Fix the granularity
        granularity = granularity || TimeGranularity.minutes;
        const format = adjustTimeFormatForGranularity(wss.timeFormat, granularity);
        const ngbTime = ngbTimeFromDate(jsDate);
        const formattedTime = formatTime(ngbTime, format, trx);
        return formattedTime;
    } else {
        return '';
    }
}

export function datetimeFormat(
    isoDate: string,
    wss: WorkspaceService,
    trx: TranslateService,
    calendar?: Calendar,
    granularity?: DateTimeGranularity) {

    if (!!isoDate) {
        const jsDate = new Date(isoDate);
        if (isNaN(jsDate.getTime())) {
            return '';
        }

        // Fix the granularity
        granularity = granularity || TimeGranularity.minutes;

        // Date
        calendar = calendar || wss.calendar;
        const dFormat = adjustDateFormatForGranularity(wss.dateFormat, granularity);
        const ngbDate = ngbDateFromDate(jsDate, calendar);
        const formattedDate = formatDate(ngbDate, dFormat, trx, calendar);

        // Time
        let formattedTime: string;
        if (granularity >= TimeGranularity.hours) {
            const tFormat = adjustTimeFormatForGranularity(wss.timeFormat, granularity);
            const ngbTime = ngbTimeFromDate(jsDate);
            formattedTime = formatTime(ngbTime, tFormat, trx);
        }

        if (!!formattedTime) {
            return `${formattedDate} ${formattedTime}`;
        } else {
            return formattedDate;
        }
    } else {
        return '';
    }
}
