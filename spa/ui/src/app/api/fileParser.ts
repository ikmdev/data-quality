/**
 * File parsing utilities for batch processing
 * Handles Excel, XLS, and CSV file reading
 */

import * as XLSX from 'xlsx';

export interface BatchRow {
  [key: string]: string | number | undefined;
}

async function parseExcelFile(file: File): Promise<BatchRow[]> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const data = new Uint8Array(e.target?.result as ArrayBuffer);
        const workbook = XLSX.read(data, { type: 'array' });
        const worksheet = workbook.Sheets[workbook.SheetNames[0]];
        const rows: any[][] = XLSX.utils.sheet_to_json(worksheet, { header: 1 });

        if (rows.length < 2) {
          throw new Error('Excel file must contain header row and at least one data row.');
        }

        const headers = rows[0].map((header) =>
          String(header || '').trim()
        );
        const parsedRows: BatchRow[] = [];

        for (let i = 1; i < rows.length; i += 1) {
          const values = rows[i] || [];
          if (values.every((cell: any) => cell === undefined || cell === null || String(cell).trim() === '')) {
            continue;
          }

          const row: BatchRow = {};
          headers.forEach((header, index) => {
            row[header] = values[index] !== undefined ? values[index] : '';
          });
          parsedRows.push(row);
        }

        resolve(parsedRows);
      } catch (err) {
        reject(err);
      }
    };
    reader.onerror = () => {
      reject(new Error('Unable to read Excel file.'));
    };
    reader.readAsArrayBuffer(file);
  });
}

function parseCSVLine(line: string): string[] {
  const values: string[] = [];
  let current = '';
  let inQuotes = false;

  for (let i = 0; i < line.length; i += 1) {
    const ch = line[i];
    if (ch === '"') {
      if (inQuotes && line[i + 1] === '"') {
        current += '"';
        i += 1;
      } else {
        inQuotes = !inQuotes;
      }
    } else if (ch === ',' && !inQuotes) {
      values.push(current);
      current = '';
    } else {
      current += ch;
    }
  }
  values.push(current);
  return values;
}

async function parseCSVFile(file: File): Promise<BatchRow[]> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const text = String(e.target?.result || '');
        const lines = text.split(/\r?\n/).filter((line) => line.trim());

        if (lines.length < 2) {
          throw new Error('CSV file must contain header row and at least one data row.');
        }

        const headers = parseCSVLine(lines[0]).map((header) => header.trim());
        const parsedRows: BatchRow[] = [];

        for (let i = 1; i < lines.length; i += 1) {
          const values = parseCSVLine(lines[i]).map((value) => value.trim());
          const row: BatchRow = {};
          headers.forEach((header, index) => {
            row[header] = values[index] !== undefined ? values[index] : '';
          });
          parsedRows.push(row);
        }

        resolve(parsedRows);
      } catch (err) {
        reject(err);
      }
    };
    reader.onerror = () => {
      reject(new Error('Unable to read CSV file.'));
    };
    reader.readAsText(file);
  });
}

export async function parseBatchFile(file: File): Promise<BatchRow[]> {
  const name = file.name.toLowerCase();
  if (name.endsWith('.xlsx') || name.endsWith('.xls')) {
    return parseExcelFile(file);
  }
  if (name.endsWith('.csv')) {
    return parseCSVFile(file);
  }
  throw new Error('Unsupported file type. Use .xlsx, .xls, or .csv.');
}
