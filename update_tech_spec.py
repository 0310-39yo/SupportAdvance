#!/usr/bin/env python3
import re

with open('docs/SharedKernel/ValueObjects/EnumValueObject_技術仕様書.md', 'r', encoding='utf-8') as f:
	content = f.read()

# Replace用途 line
old_usage = '| 用途 | 等価性判定、ToString 判定 |'
new_usage = '| 用途 | 等価性判定、ToString 判定、**0 vs null の識別** |'
if old_usage in content:
	content = content.replace(old_usage, new_usage)
	print("✓ Updated usage line")
else:
	print("✗ Usage line not found")

# Replace設計判断 block
old_design = '''**設計判断**

- EnumValueObject は常に値を持つため、IsSet は常に true
- IOptionalValueObject を実装しない（具体型で static abstract メソッドを提供）
- ValueObject 基礎との整合性を保つため、IsSet プロパティは継承元から取得'''

new_design = '''**設計判断：デフォルト値 + IsSet による 0 vs null 識別**

- **コア概念**: `ValueField = default(0)` でも、`IsSet` フラグにより実際の意味を区別する
  - `IsSet=true` + `ValueField=0` → 値確定状態。例：`Unknown=0`（不明は有効な選択肢）
  - `IsSet=false` + `ValueField=0`（default）→ **未設定状態**。NULL に相当。例：`CarModel.Unset()`
  - **重要**: 初期値 0 そのものは区別の根拠ではなく、`IsSet` フラグが判定の根拠
- EnumValueObject は常に値を持つため、IsSet は常に true
- IOptionalValueObject を実装しない（具体型で static abstract メソッドを提供）
- ValueObject 基礎との整合性を保つため、IsSet プロパティは継承元から取得'''

if old_design in content:
	content = content.replace(old_design, new_design)
	print("✓ Updated design decision block")
else:
	print("✗ Design block not found")

with open('docs/SharedKernel/ValueObjects/EnumValueObject_技術仕様書.md', 'w', encoding='utf-8') as f:
	f.write(content)

print("✓ Technical spec updated successfully")
