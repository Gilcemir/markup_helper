SHELL := /usr/bin/env bash
.SHELLFLAGS := -euo pipefail -c

CLI_PROJECT := DocFormatter.Cli
SOLUTION    := DocFormatter.sln
EXAMPLES    := examples
DEFAULT_FILE := $(EXAMPLES)/1_AR_5449_2.docx
PHASE2_BEFORE := $(EXAMPLES)/phase-2/before
PHASE2_AFTER  := $(EXAMPLES)/phase-2/after
MAC_PUBLISH := $(CLI_PROJECT)/bin/Release/net10.0/osx-arm64/publish
MAC_BIN     := $(MAC_PUBLISH)/docformatter

FILE ?= $(DEFAULT_FILE)

.PHONY: help build test test-watch run run-all phase2 phase2-all phase2-verify publish-mac publish-win release clean format logs

help:
	@echo "Targets:"
	@echo "  build              dotnet build (Debug)"
	@echo "  test               dotnet test (solution)"
	@echo "  test-watch         dotnet watch test"
	@echo "  run                run CLI on FILE=<path> (default: $(DEFAULT_FILE))"
	@echo "  run-all            run CLI in batch mode on $(EXAMPLES)/"
	@echo "  phase2             run Phase 2 pipeline on FILE=<path> (default: $(DEFAULT_FILE))"
	@echo "  phase2-all         run Phase 2 pipeline in batch mode on $(PHASE2_BEFORE)/"
	@echo "  phase2-verify      diff Phase 2 output of $(PHASE2_BEFORE) against $(PHASE2_AFTER)"
	@echo "  publish-mac        self-contained osx-arm64 binary -> $(MAC_BIN)"
	@echo "  publish-win        delegate to $(CLI_PROJECT)/publish.sh"
	@echo "  release VERSION=vX.Y.Z   tag and push, triggering the CI release workflow"
	@echo "  format             dotnet format"
	@echo "  logs               tail latest formatted/_app.log under $(EXAMPLES)/"
	@echo "  clean              remove bin/, obj/, $(EXAMPLES)/**/formatted/, and $(EXAMPLES)/**/formatted-phase2/"

build:
	dotnet build $(SOLUTION)

test:
	dotnet test $(SOLUTION)

test-watch:
	dotnet watch --project DocFormatter.Tests test

run:
	dotnet run --project $(CLI_PROJECT) -- "$(FILE)"

run-all:
	dotnet run --project $(CLI_PROJECT) -- "$(EXAMPLES)"

phase2:
	dotnet run --project $(CLI_PROJECT) -- phase2 "$(FILE)"

phase2-all:
	dotnet run --project $(CLI_PROJECT) -- phase2 "$(PHASE2_BEFORE)"

phase2-verify:
	dotnet run --project $(CLI_PROJECT) -- phase2-verify "$(PHASE2_BEFORE)" "$(PHASE2_AFTER)"

publish-mac:
	dotnet publish $(CLI_PROJECT) \
		-c Release \
		-r osx-arm64 \
		--self-contained true \
		-p:PublishSingleFile=true \
		-p:IncludeNativeLibrariesForSelfExtract=true \
		-p:PublishTrimmed=false
	@echo "Artifact: $(MAC_BIN)"

publish-win:
	./$(CLI_PROJECT)/publish.sh

release:
	@if [[ -z "$${VERSION:-}" ]]; then \
		echo "Usage: make release VERSION=vX.Y.Z" >&2; exit 1; \
	fi
	@if [[ ! "$${VERSION}" =~ ^v[0-9]+\.[0-9]+\.[0-9]+$$ ]]; then \
		echo "VERSION must be vMAJOR.MINOR.PATCH (got '$${VERSION}')" >&2; exit 1; \
	fi
	@if [[ -n "$$(git status --porcelain)" ]]; then \
		echo "Working tree is not clean. Commit or stash first." >&2; exit 1; \
	fi
	@if git rev-parse "$${VERSION}" >/dev/null 2>&1; then \
		echo "Tag $${VERSION} already exists." >&2; exit 1; \
	fi
	@branch="$$(git rev-parse --abbrev-ref HEAD)"; \
	if [[ "$${branch}" != "main" ]]; then \
		echo "Refusing to release from branch '$${branch}' (must be main)." >&2; exit 1; \
	fi
	git tag -a "$${VERSION}" -m "Release $${VERSION}"
	git push origin "$${VERSION}"
	@echo ""
	@echo "Tag $${VERSION} pushed."
	@echo "CI: https://github.com/Gilcemir/markup_helper/actions"
	@echo "Release will appear at: https://github.com/Gilcemir/markup_helper/releases/tag/$${VERSION}"

format:
	dotnet format $(SOLUTION)

logs:
	@latest=$$(ls -t $(EXAMPLES)/formatted/_app.log $(EXAMPLES)/**/formatted/_app.log 2>/dev/null | head -1); \
	if [[ -z "$$latest" ]]; then echo "No _app.log found under $(EXAMPLES)/"; exit 1; fi; \
	echo "==> $$latest"; tail -n 100 "$$latest"

clean:
	dotnet clean $(SOLUTION) || true
	find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
	if [ -d "$(EXAMPLES)" ]; then \
		find "$(EXAMPLES)" -type d \( -name formatted -o -name formatted-phase2 \) -prune -exec rm -rf {} +; \
	fi
