---
name: phase3f-entities-win
expert-role: Data Architect
domain-expertise: Entity modeling, ERD, database design
---

# Phase 3f: Entity Definitions

## Expert Role

You are a **Data Architect** with deep expertise in:
- Entity-relationship modeling
- Database design and normalization
- Domain-driven design
- Data dictionary creation

## Purpose

Extract and document all domain entities with attributes and relationships.

## Inputs

- `{folder}/shared/requirements-normalized.json` - Normalized requirements
- `{folder}/shared/sample-data-analysis.json` - Sample data analysis
- `{folder}/shared/domain-context.json` - Domain context

## Required Outputs

- `{folder}/outputs/ENTITY_DEFINITIONS.md` - Entity specifications

## Instructions

### Step 1: Load Data

```python
requirements = read_json(f"{folder}/shared/requirements-normalized.json")
sample_data = read_json(f"{folder}/shared/sample-data-analysis.json")
domain_context = read_json(f"{folder}/shared/domain-context.json")
```

### Step 2: Extract Entities from Multiple Sources

```python
def extract_entities(requirements, sample_data, domain_context):
    """Extract entities from all available sources."""
    entities = {}

    # From sample data analysis
    for entity in sample_data.get("entities", []):
        name = entity.get("name", "Unknown")
        entities[name] = {
            "name": name,
            "source": "sample_data",
            "attributes": [],
            "relationships": [],
            "primary_key": entity.get("primary_key"),
            "foreign_keys": entity.get("foreign_keys", [])
        }

    # From field definitions
    for field in sample_data.get("field_definitions", []):
        entity_name = field.get("source_sheet", "Unknown")
        if entity_name not in entities:
            entities[entity_name] = {
                "name": entity_name,
                "source": "field_definitions",
                "attributes": [],
                "relationships": []
            }
        entities[entity_name]["attributes"].append({
            "name": field["field_name"],
            "type": field["data_type"],
            "nullable": field.get("nullable", True),
            "validation": field.get("validation_rules", [])
        })

    # From requirements (entity mentions)
    entity_patterns = [
        r'\b([A-Z][a-z]+(?:[A-Z][a-z]+)*)\s+(?:record|entity|object|table)\b',
        r'\bthe\s+([A-Z][a-z]+(?:\s+[A-Z][a-z]+)?)\b'
    ]

    for req in requirements.get("requirements", []):
        text = req.get("text", "")
        for pattern in entity_patterns:
            matches = re.findall(pattern, text)
            for match in matches:
                if match not in entities and len(match) > 2:
                    entities[match] = {
                        "name": match,
                        "source": "requirements",
                        "attributes": [],
                        "relationships": [],
                        "requirement_refs": [req.get("canonical_id")]
                    }

    return entities

entities = extract_entities(requirements, sample_data, domain_context)
```

### Step 3: Add Domain-Specific Entities

```python
DOMAIN_ENTITIES = {
    "education": {
        "Student": ["StudentID", "FirstName", "LastName", "DOB", "Grade", "EnrollmentDate"],
        "Enrollment": ["EnrollmentID", "StudentID", "SchoolID", "Grade", "StartDate", "EndDate"],
        "School": ["SchoolID", "Name", "DistrictID", "Address", "Principal"],
        "District": ["DistrictID", "Name", "ESDID", "Superintendent"],
        "Staff": ["StaffID", "FirstName", "LastName", "Position", "SchoolID"],
        "Course": ["CourseID", "Name", "Credits", "Subject", "GradeLevel"]
    },
    "healthcare": {
        "Patient": ["PatientID", "FirstName", "LastName", "DOB", "MRN"],
        "Encounter": ["EncounterID", "PatientID", "ProviderID", "Date", "Type"],
        "Provider": ["ProviderID", "Name", "NPI", "Specialty"],
        "Diagnosis": ["DiagnosisID", "EncounterID", "ICD10Code", "Description"],
        "Medication": ["MedicationID", "PatientID", "DrugName", "Dosage", "StartDate"]
    }
}

domain = domain_context.get("selected_domain", "default")
if domain in DOMAIN_ENTITIES:
    for entity_name, attributes in DOMAIN_ENTITIES[domain].items():
        if entity_name not in entities:
            entities[entity_name] = {
                "name": entity_name,
                "source": "domain_profile",
                "attributes": [{"name": attr, "type": "inferred"} for attr in attributes],
                "relationships": []
            }
```

### Step 4: Infer Relationships

```python
def infer_relationships(entities):
    """Infer relationships based on naming conventions."""
    for entity_name, entity in entities.items():
        for attr in entity.get("attributes", []):
            attr_name = attr.get("name", "")
            # Foreign key pattern: EntityID
            if attr_name.endswith("ID") and attr_name != f"{entity_name}ID":
                related_entity = attr_name[:-2]  # Remove "ID"
                if related_entity in entities:
                    entity["relationships"].append({
                        "type": "belongs_to",
                        "target": related_entity,
                        "foreign_key": attr_name
                    })
                    # Add inverse relationship
                    if "relationships" not in entities[related_entity]:
                        entities[related_entity]["relationships"] = []
                    entities[related_entity]["relationships"].append({
                        "type": "has_many",
                        "target": entity_name,
                        "foreign_key": attr_name
                    })

    return entities

entities = infer_relationships(entities)
```

### Step 5: Generate Entity Documentation

```python
def generate_entities_md(entities, domain):
    doc = f"""# Entity Definitions

**Domain:** {domain}
**Generated:** {datetime.now().strftime('%Y-%m-%d %H:%M')}
**Total Entities:** {len(entities)}

---

## Entity Relationship Diagram (Conceptual)

```
"""

    # Simple ERD representation
    for name, entity in list(entities.items())[:10]:
        doc += f"[{name}]"
        for rel in entity.get("relationships", [])[:2]:
            if rel["type"] == "has_many":
                doc += f" --< [{rel['target']}]"
        doc += "\n"

    doc += """```

---

## Entity Catalog

"""

    # Document each entity
    for name, entity in sorted(entities.items()):
        doc += f"""### {name}

**Source:** {entity.get('source', 'unknown')}

#### Attributes
| Attribute | Type | Nullable | Validation |
|-----------|------|----------|------------|
"""

        for attr in entity.get("attributes", [])[:15]:
            nullable = "Yes" if attr.get("nullable", True) else "No"
            validation = ", ".join(attr.get("validation", [])) or "-"
            doc += f"| {attr.get('name', 'N/A')} | {attr.get('type', 'N/A')} | {nullable} | {validation[:30]} |\n"

        doc += "\n#### Relationships\n"
        for rel in entity.get("relationships", []):
            doc += f"- **{rel['type']}** {rel['target']} (via {rel.get('foreign_key', 'N/A')})\n"

        doc += "\n---\n\n"

    # Add data dictionary
    doc += """
## Data Dictionary Summary

| Entity | Attributes | Relationships |
|--------|------------|---------------|
"""

    for name, entity in sorted(entities.items()):
        attr_count = len(entity.get("attributes", []))
        rel_count = len(entity.get("relationships", []))
        doc += f"| {name} | {attr_count} | {rel_count} |\n"

    doc += """

---

## Appendix: Attribute Index

| Attribute | Entity | Type |
|-----------|--------|------|
"""

    # All attributes alphabetically
    all_attrs = []
    for name, entity in entities.items():
        for attr in entity.get("attributes", []):
            all_attrs.append((attr.get("name", "N/A"), name, attr.get("type", "N/A")))

    for attr_name, entity_name, attr_type in sorted(all_attrs)[:50]:
        doc += f"| {attr_name} | {entity_name} | {attr_type} |\n"

    return doc

entities_md = generate_entities_md(entities, domain)
write_file(f"{folder}/outputs/ENTITY_DEFINITIONS.md", entities_md)
```

## Quality Checklist

- [ ] `ENTITY_DEFINITIONS.md` created in `outputs/`
- [ ] 40+ entities documented
- [ ] Attributes listed for each entity
- [ ] Relationships inferred
- [ ] Data dictionary included
- [ ] ERD diagram (conceptual) included
