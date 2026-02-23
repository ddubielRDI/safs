---
name: phase2.5-sample-win
expert-role: Data Analyst
domain-expertise: Data profiling, sample data analysis
---

# Phase 2.5: Sample Data Analysis

## Expert Role

You are a **Data Analyst** with deep expertise in:
- Data profiling and quality assessment
- Sample data analysis and pattern detection
- Data dictionary creation
- Field type inference and validation rules

## Purpose

Analyze sample data files to extract data specifications, field definitions, and validation requirements.

## Inputs

- `{folder}/flattened/*.md` - Flattened documents (may contain data samples)
- `{folder}/original/*.xlsx` - Original spreadsheets with sample data
- `{folder}/shared/domain-context.json` - Domain context

## Required Outputs

- `{folder}/shared/sample-data-analysis.json` - Data specifications and field definitions

## Instructions

### Step 1: Identify Data Files

```python
import glob
import os

# Find potential data files
xlsx_files = glob.glob(f"{folder}/original/*.xlsx")
csv_files = glob.glob(f"{folder}/original/*.csv")
flattened_files = glob.glob(f"{folder}/flattened/*.md")

# Also look for data tables in flattened markdown
data_sources = {
    "spreadsheets": xlsx_files + csv_files,
    "embedded_tables": []
}
```

### Step 2: Extract Tables from Markdown

```python
def extract_data_tables(content):
    """Extract data tables from markdown content."""
    tables = []

    # Markdown table pattern
    table_pattern = r'\|[^\n]+\|\n\|[\-\s\|]+\|\n(?:\|[^\n]+\|\n)+'
    matches = re.findall(table_pattern, content)

    for table_text in matches:
        rows = table_text.strip().split('\n')
        if len(rows) >= 3:  # Header + separator + at least one data row
            header = [cell.strip() for cell in rows[0].split('|')[1:-1]]
            data_rows = []
            for row in rows[2:]:
                cells = [cell.strip() for cell in row.split('|')[1:-1]]
                if len(cells) == len(header):
                    data_rows.append(dict(zip(header, cells)))

            if data_rows:
                tables.append({
                    "header": header,
                    "row_count": len(data_rows),
                    "sample_rows": data_rows[:5]
                })

    return tables

for file_path in flattened_files:
    content = read_file(file_path)
    tables = extract_data_tables(content)
    for table in tables:
        table["source"] = os.path.basename(file_path)
    data_sources["embedded_tables"].extend(tables)
```

### Step 3: Analyze Spreadsheets

```python
import openpyxl

def analyze_spreadsheet(file_path):
    """Analyze spreadsheet structure and sample data."""
    wb = openpyxl.load_workbook(file_path, data_only=True)
    sheets = []

    for sheet_name in wb.sheetnames:
        sheet = wb[sheet_name]
        rows = list(sheet.iter_rows(values_only=True))

        if not rows:
            continue

        # Assume first row is header
        header = [str(cell) if cell else f"Column_{i}" for i, cell in enumerate(rows[0])]
        data_rows = rows[1:100]  # Sample first 100 rows

        # Analyze each column
        columns = []
        for col_idx, col_name in enumerate(header):
            values = [row[col_idx] for row in data_rows if row[col_idx] is not None]

            columns.append({
                "name": col_name,
                "sample_values": values[:5],
                "non_null_count": len(values),
                "total_rows": len(data_rows),
                "inferred_type": infer_type(values),
                "unique_values": len(set(str(v) for v in values)),
                "patterns": detect_patterns(values)
            })

        sheets.append({
            "name": sheet_name,
            "row_count": len(rows) - 1,
            "columns": columns
        })

    return sheets

def infer_type(values):
    """Infer data type from sample values."""
    if not values:
        return "UNKNOWN"

    # Check if all numeric
    numeric_count = sum(1 for v in values if isinstance(v, (int, float)) or (isinstance(v, str) and v.replace('.', '').replace('-', '').isdigit()))
    if numeric_count / len(values) > 0.9:
        return "NUMERIC"

    # Check if all dates
    date_patterns = [r'\d{1,2}/\d{1,2}/\d{2,4}', r'\d{4}-\d{2}-\d{2}']
    date_count = 0
    for v in values:
        if any(re.match(p, str(v)) for p in date_patterns):
            date_count += 1
    if date_count / len(values) > 0.8:
        return "DATE"

    # Check if boolean
    bool_values = {'yes', 'no', 'true', 'false', 'y', 'n', '1', '0'}
    bool_count = sum(1 for v in values if str(v).lower() in bool_values)
    if bool_count / len(values) > 0.9:
        return "BOOLEAN"

    # Check if code/enum (few unique values)
    if len(set(str(v) for v in values)) <= min(10, len(values) * 0.3):
        return "CODE"

    return "TEXT"

def detect_patterns(values):
    """Detect patterns in values."""
    patterns = []

    str_values = [str(v) for v in values if v]
    if not str_values:
        return patterns

    # Check for consistent length
    lengths = [len(v) for v in str_values]
    if len(set(lengths)) == 1:
        patterns.append(f"FIXED_LENGTH_{lengths[0]}")

    # Check for common prefixes
    if all(v.startswith(str_values[0][:2]) for v in str_values):
        patterns.append(f"PREFIX_{str_values[0][:2]}")

    # Check for email pattern
    if all('@' in v for v in str_values):
        patterns.append("EMAIL")

    # Check for phone pattern
    phone_pattern = r'\d{3}[-.\s]?\d{3}[-.\s]?\d{4}'
    if all(re.match(phone_pattern, v) for v in str_values):
        patterns.append("PHONE")

    return patterns

# Analyze all spreadsheets
spreadsheet_analysis = []
for xlsx_file in xlsx_files:
    try:
        analysis = analyze_spreadsheet(xlsx_file)
        spreadsheet_analysis.append({
            "file": os.path.basename(xlsx_file),
            "sheets": analysis
        })
    except Exception as e:
        spreadsheet_analysis.append({
            "file": os.path.basename(xlsx_file),
            "error": str(e)
        })
```

### Step 4: Generate Field Definitions

```python
def generate_field_definitions(column_analysis):
    """Generate field definition from column analysis."""
    return {
        "field_name": column_analysis["name"],
        "data_type": column_analysis["inferred_type"],
        "nullable": column_analysis["non_null_count"] < column_analysis["total_rows"],
        "unique_constraint": column_analysis["unique_values"] == column_analysis["non_null_count"],
        "patterns": column_analysis["patterns"],
        "sample_values": column_analysis["sample_values"],
        "validation_rules": generate_validation_rules(column_analysis)
    }

def generate_validation_rules(column):
    """Generate validation rules from column analysis."""
    rules = []

    if column["inferred_type"] == "NUMERIC":
        values = [v for v in column["sample_values"] if isinstance(v, (int, float))]
        if values:
            rules.append(f"RANGE: {min(values)} - {max(values)}")

    if column["inferred_type"] == "CODE":
        unique = list(set(str(v) for v in column["sample_values"]))
        rules.append(f"ALLOWED_VALUES: {unique[:10]}")

    if "FIXED_LENGTH" in str(column["patterns"]):
        length = int(column["patterns"][0].split('_')[-1])
        rules.append(f"LENGTH: {length}")

    if column["inferred_type"] == "DATE":
        rules.append("FORMAT: date")

    return rules

# Generate definitions for all columns
field_definitions = []
for spreadsheet in spreadsheet_analysis:
    for sheet in spreadsheet.get("sheets", []):
        for column in sheet.get("columns", []):
            field_def = generate_field_definitions(column)
            field_def["source_file"] = spreadsheet["file"]
            field_def["source_sheet"] = sheet["name"]
            field_definitions.append(field_def)
```

### Step 5: Identify Data Entities

```python
def identify_entities(spreadsheets, domain_context):
    """Identify potential data entities from spreadsheet structure."""
    entities = []

    for spreadsheet in spreadsheets:
        for sheet in spreadsheet.get("sheets", []):
            # Sheet name often indicates entity
            entity_name = sheet["name"].replace("_", " ").title()

            # Look for ID columns (entity identifiers)
            id_columns = [c for c in sheet["columns"] if "id" in c["name"].lower() or c["name"].lower().endswith("key")]

            # Identify relationships
            foreign_keys = [c for c in sheet["columns"] if c["name"].lower().endswith("id") and c["name"].lower() != f"{entity_name.lower()}id"]

            entities.append({
                "name": entity_name,
                "source": f"{spreadsheet['file']}/{sheet['name']}",
                "primary_key": id_columns[0]["name"] if id_columns else None,
                "foreign_keys": [fk["name"] for fk in foreign_keys],
                "field_count": len(sheet["columns"]),
                "row_count": sheet["row_count"]
            })

    return entities

entities = identify_entities(spreadsheet_analysis, domain_context)
```

### Step 6: Write Output

```python
sample_data_output = {
    "analyzed_at": datetime.now().isoformat(),
    "summary": {
        "spreadsheets_analyzed": len(spreadsheet_analysis),
        "embedded_tables_found": len(data_sources["embedded_tables"]),
        "total_fields_defined": len(field_definitions),
        "entities_identified": len(entities)
    },
    "spreadsheet_analysis": spreadsheet_analysis,
    "embedded_tables": data_sources["embedded_tables"],
    "field_definitions": field_definitions,
    "entities": entities,
    "validation_rules_summary": {
        "total_rules": sum(len(f["validation_rules"]) for f in field_definitions),
        "by_type": {
            t: sum(1 for f in field_definitions if f["data_type"] == t)
            for t in set(f["data_type"] for f in field_definitions)
        }
    }
}

write_json(f"{folder}/shared/sample-data-analysis.json", sample_data_output)
```

### Step 7: Report Results

```
📊 Sample Data Analysis Complete
=================================
Spreadsheets Analyzed: {count}
Embedded Tables Found: {count}
Total Fields Defined: {len(field_definitions)}
Entities Identified: {len(entities)}

Field Type Distribution:
| Type | Count |
|------|-------|
{table rows}

Entities Found:
{entity_list}

Validation Rules Generated: {total_rules}
```

## Quality Checklist

- [ ] `sample-data-analysis.json` created in `shared/`
- [ ] All spreadsheets analyzed
- [ ] Embedded tables extracted from markdown
- [ ] Field types inferred
- [ ] Validation rules generated
- [ ] Entities identified with relationships
