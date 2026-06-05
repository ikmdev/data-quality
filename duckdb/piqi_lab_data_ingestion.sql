SELECT json_object(
               'messageId', Unique_ID,
               'formatID', '',
               'useCaseID', '',
               'patient', json_object(
                       'id', Unique_ID,
                       'labResults', json_array(
                               json_object(
                                       'test', json_object(
                                       'codings', CASE
                                                      WHEN LOINC IS NOT NULL AND LOINC != ''
                                       THEN json_array(json_object('code', LOINC, 'display', LabChemTestName, 'system',
                                                                   '2.16.840.1.113883.6.1'))
                                                      ELSE json_array()
                                           END,
                                       'text', COALESCE(LabChemTestName, '')
                                               ),
                                       'performingSite', json_object(
                                               'text', COALESCE(AccessioningInstitution, '')
                                                         ),
                                       'specimenType', json_object(
                                               'text', COALESCE(Topography, '')
                                                       ),
                                       'abnormalFlag', json_object(
                                               'codings', CASE
                                                              WHEN Abnormal IS NOT NULL AND Abnormal != ''
                                       THEN json_array(json_object('code', Abnormal, 'display', Abnormal, 'system', 'AbFlag'))
                                                              ELSE json_array()
                                           END,
                                               'text', COALESCE(Abnormal, '')
                                                       ),
                                       'referenceRange', CASE
                                                             WHEN NULLIF(RefLow, '') IS NOT NULL AND NULLIF(RefHigh, '') IS NOT NULL
                                                                 THEN json_object('text',
                                                                                  COALESCE(RefLow, '') || ' - ' ||
                                                                                  COALESCE(RefHigh, ''), 'lowValue',
                                                                                  RefLow, 'highValue', RefHigh)
                                                             WHEN NULLIF(RefLow, '') IS NOT NULL
                                                                 THEN json_object('text',
                                                                                  COALESCE(RefLow, '') || ' - ' ||
                                                                                  COALESCE(RefHigh, ''), 'lowValue',
                                                                                  RefLow)
                                                             WHEN NULLIF(RefHigh, '') IS NOT NULL
                                                                 THEN json_object('text',
                                                                                  COALESCE(RefLow, '') || ' - ' ||
                                                                                  COALESCE(RefHigh, ''), 'highValue',
                                                                                  RefHigh)
                                                             ELSE
                                                                 json_object('text', COALESCE(RefLow, '') || ' - ' ||
                                                                                     COALESCE(RefHigh, ''))
                                           END,
                                       'resultValue', json_object(
                                               'codings', CASE
                                                              WHEN LabChemResultValue IS NOT NULL AND LabChemResultValue != ''
                                       THEN json_array(json_object('code', LabChemResultValue, 'display',
                                                                   LabChemResultValue, 'system', 'ResVal'))
                                                              ELSE json_array()
                                           END,
                                               'text', COALESCE(LabChemResultValue, '')
                                                      ),
                                       'resultUnit', json_object(
                                               'codings', CASE
                                                              WHEN Units IS NOT NULL AND Units != ''
                                       THEN json_array(json_object('code', Units, 'display', Units, 'system', 'UCUM'))
                                                              ELSE json_array()
                                           END,
                                               'text', COALESCE(Units, '')
                                                     ),
                                       'interpretation', json_object(
                                               'codings', json_array(
                                               json_object(
                                                       'code', COALESCE(NULLIF(Abnormal, ''), 'N'),
                                                       'system', '2.16.840.1.113883.5.83'
                                               )
                                                          ),
                                               'text', COALESCE(NULLIF(Abnormal, ''), 'N')
                                                         )
                               )
                                     )
                          ),
               'dataSourceID', '${SOURCE_ID}',
               'dataProviderID', '${PROVIDER_ID}',
               'messageID', Unique_ID
       ) AS payload_json
FROM read_csv_auto('${SOURCE_FILE}', all_varchar = true);