import { DashboardDefinitionWidgetForClient } from '~/app/data/dto/definitions-for-client';

export const tileWidth = 260;
export const tileHeight = 180;

export const defaultWidth = 2;
export const defaultHeight = 2;

export const maxSize = 16;
export const maxOffset = 1000;

/**
 * If 2 widgets overlap, this calculates how many tiles widget #2 needs to
 * be shifted down (in the Y direction) to stop overlapping with widget #1
 */
export const overlapY = (x1: number, y1: number, w1: number, h1: number, x2: number, y2: number, w2: number, h2: number): number =>
    (x1 + w1) > x2 && x1 < (x2 + w2) && (y1 + h1) > y2 && y1 < (y2 + h2) ? (y1 + h1 - y2) : 0;

export function rearrange(modifiedWidget: DashboardDefinitionWidgetForClient, otherWidgets: DashboardDefinitionWidgetForClient[]) {

    // Start off with some cleanup
    cleanupWidgetPreviews(otherWidgets);
    const tocheck: DashboardDefinitionWidgetForClient[] = [modifiedWidget];
    // otherWidgets = otherWidgets.slice();
    // otherWidgets.sort((w1, w2) => w1.OffsetY - w2.OffsetY);

    while (tocheck.length > 0) {
        const w1 = tocheck.shift();
        for (const w2 of otherWidgets) {
            if (w2 === w1) {
                continue;
            }

            const y1 = w1.OffsetY + (w1.changeY || 0);
            const y2 = w2.OffsetY + (w2.changeY || 0);
            const changeY = overlapY(w1.OffsetX, y1, w1.Width, w1.Height, w2.OffsetX, y2, w2.Width, w2.Height);
            if (changeY > 0) {
                w2.changeY = (w2.changeY || 0) + changeY;
                tocheck.push(w2);
            }
        }
    }
}

export function cleanupWidgetPreviews(widgets: DashboardDefinitionWidgetForClient[]) {
    // Start off with some cleanup
    for (const w of widgets) {
        delete w.changeY;
    }
}
