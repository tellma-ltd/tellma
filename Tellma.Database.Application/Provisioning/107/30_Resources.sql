DELETE FROM @Resources; DELETE FROM @ResourceUnits;
INSERT INTO @Resources([Index],
[Code],     [Name],			[Name2],			[Decimal1]) VALUES(
0,N'107L1', N'Soba 51 Land', N'أرض سوبا 51',	50000);

UPDATE @Resources 
SET [LocationJson] = 
N'{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "properties": {},
      "geometry": {
        "type": "Polygon",
        "coordinates": [
          [
            [
              32.51952052116394,
              15.59563333643099
            ],
            [
              32.51785755157471,
              15.592967190168391
            ],
            [
              32.518254518508904,
              15.59277084469494
            ],
            [
              32.51901626586914,
              15.593070529815831
            ],
            [
              32.520132064819336,
              15.594878965225904
            ],
            [
              32.520636320114136,
              15.595044307097114
            ],
            [
              32.51952052116394,
              15.59563333643099
            ]
          ]
        ]
      }
    }
  ]
}'
WHERE [Index] = 0;

EXEC api.Resources__Save
	@DefinitionId = @LandMemberRD,
	@Entities = @Resources,
	@ResourceUnits = @ResourceUnits,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print '107 Lands: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;