import { describe, expect, it } from "@jest/globals";
import { addSelect } from "./__test_helpers__";
import { validateApplyAfter } from "./validation";

const PREFIX = "base2edit_stage_2_";

// validateApplyAfter appends its error node to the nearest `.auto-input` ancestor
// (via SwarmUI's global findParentOfClass), so the select must live inside one.
const buildApplyAfterSelect = (
    value: string,
    options: string[],
): HTMLSelectElement => {
    const wrap = document.createElement("div");
    wrap.className = "auto-input";
    document.body.appendChild(wrap);
    const sel = addSelect(`${PREFIX}applyafter`, value, options);
    wrap.appendChild(sel);
    return sel;
};

describe("validateApplyAfter", () => {
    it("flags a stale SeedVR2 selection as invalid when SeedVR2 is unavailable", () => {
        const sel = buildApplyAfterSelect("SeedVR2", ["SeedVR2", "Refiner"]);

        validateApplyAfter(PREFIX, [0, 1, 2], 2, false);

        expect(sel.classList.contains("is-invalid")).toBe(true);
        expect(
            document.getElementById(`${PREFIX}applyafter_error`),
        ).not.toBeNull();
    });

    it("does not flag a SeedVR2 selection when SeedVR2 is available", () => {
        const sel = buildApplyAfterSelect("SeedVR2", ["SeedVR2", "Refiner"]);

        validateApplyAfter(PREFIX, [0, 1, 2], 2, true);

        expect(sel.classList.contains("is-invalid")).toBe(false);
        expect(document.getElementById(`${PREFIX}applyafter_error`)).toBeNull();
    });

    it("does not flag Refiner regardless of SeedVR2 availability", () => {
        const sel = buildApplyAfterSelect("Refiner", ["Refiner", "SeedVR2"]);

        validateApplyAfter(PREFIX, [0, 1, 2], 2, false);

        expect(sel.classList.contains("is-invalid")).toBe(false);
        expect(document.getElementById(`${PREFIX}applyafter_error`)).toBeNull();
    });

    it("still flags a missing Edit Stage ref (existing behavior preserved)", () => {
        const sel = buildApplyAfterSelect("Edit Stage 9", [
            "Edit Stage 9",
            "Refiner",
        ]);

        validateApplyAfter(PREFIX, [0, 1, 2], 2, true);

        expect(sel.classList.contains("is-invalid")).toBe(true);
        expect(
            document.getElementById(`${PREFIX}applyafter_error`),
        ).not.toBeNull();
    });
});
