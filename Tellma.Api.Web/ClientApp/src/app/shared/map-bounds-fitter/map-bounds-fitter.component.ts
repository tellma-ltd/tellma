// import { Component, forwardRef, Input } from '@angular/core';
// import { FitBoundsAccessor, FitBoundsDetails, LatLngLiteral } from '@agm/core';
// import { Observable, Subject } from 'rxjs';
// import { xMin, xMax, yMin, yMax } from 'geojson-bounds';

// @Component({
//   selector: 't-map-bounds-fitter',
//   template: '',
//   providers: [
//     { provide: FitBoundsAccessor, useExisting: forwardRef(() => MapBoundsFitterComponent) }
//   ],
// })
// export class MapBoundsFitterComponent implements FitBoundsAccessor {

//   _fitBoundsDetails$ = new Subject<FitBoundsDetails>();
//   _geoJson: any;

//   @Input()
//   public type: 'max' | 'min';

//   @Input()
//   public set geoJson(v: any) {
//     this._geoJson = v;

//     try {
//       // Retrieve the max or the min depending on config
//       let point: LatLngLiteral;
//       if (this.type === 'max') {
//         point = { lat: yMax(v), lng: xMax(v) };
//       } else {
//         point = { lat: yMin(v), lng: xMin(v) };
//       }

//       // If no max or min could be calculated: ignore
//       if (!point.lat || !point.lng || Math.abs(point.lat) === Infinity || Math.abs(point.lng) === Infinity) {
//         return;
//       }

//       // Notify the map to adjust boundaries
//       setTimeout(() => {
//         this._fitBoundsDetails$.next({ latLng: point });
//       });
//     } catch {}
//   }

//   public get geoJson(): any {
//     return this._geoJson;
//   }

//   getFitBoundsDetails$(): Observable<FitBoundsDetails> {
//     return this._fitBoundsDetails$;
//   }

// }
