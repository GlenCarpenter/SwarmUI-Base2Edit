import { describe, expect, it } from "@jest/globals";
import { addSelect } from "./__test_helpers__";
import { cleanApplyAfterOptions } from "./validation";

const optionValues = (sel: HTMLSelectElement): string[] =>
    Array.from(sel.options).map((o) => o.value);

const findOption = (sel: HTMLSelectElement, value: string) =>
    Array.from(sel.options).find((o) => o.value === value);

describe("cleanApplyAfterOptions", () => {
    it("keeps the SeedVR2 option enabled when seedVr2Available is true", () => {
        const sel = addSelect("ap", "Refiner", [
            "Refiner",
            "SeedVR2",
            "Edit Stage 1",
        ]);

        cleanApplyAfterOptions(sel, [0, 1, 2], 2, true);

        expect(optionValues(sel)).toContain("SeedVR2");
        const seed = findOption(sel, "SeedVR2");
        expect(seed?.hidden).toBe(false);
        expect(seed?.disabled).toBe(false);
    });

    it("removes an unselected SeedVR2 option when seedVr2Available is false", () => {
        const sel = addSelect("ap", "Refiner", [
            "Refiner",
            "SeedVR2",
            "Edit Stage 1",
        ]);

        cleanApplyAfterOptions(sel, [0, 1, 2], 2, false);

        expect(optionValues(sel)).not.toContain("SeedVR2");
    });

    it("hides (not removes) a selected SeedVR2 option when seedVr2Available is false", () => {
        const sel = addSelect("ap", "SeedVR2", [
            "Refiner",
            "SeedVR2",
            "Edit Stage 1",
        ]);

        cleanApplyAfterOptions(sel, [0, 1, 2], 2, false);

        const seed = findOption(sel, "SeedVR2");
        expect(seed).toBeDefined();
        expect(seed?.hidden).toBe(true);
        expect(seed?.disabled).toBe(true);
    });

    it("treats seedVr2Available as false by default", () => {
        const sel = addSelect("ap", "Refiner", ["Refiner", "SeedVR2"]);

        cleanApplyAfterOptions(sel, [0, 1], 1);

        expect(optionValues(sel)).not.toContain("SeedVR2");
    });

    it("resets a selected-but-unavailable SeedVR2 back to Refiner (keeping the hidden option)", () => {
        const sel = addSelect("ap", "SeedVR2", [
            "Refiner",
            "SeedVR2",
            "Edit Stage 1",
        ]);

        cleanApplyAfterOptions(sel, [0, 1, 2], 2, false);

        expect(sel.value).toBe("Refiner");
        const seed = findOption(sel, "SeedVR2");
        expect(seed).toBeDefined();
        expect(seed?.hidden).toBe(true);
        expect(seed?.disabled).toBe(true);
    });

    it("does not clobber a valid Edit Stage selection while removing an unavailable SeedVR2", () => {
        const sel = addSelect("ap", "Edit Stage 1", [
            "Refiner",
            "SeedVR2",
            "Edit Stage 1",
        ]);

        cleanApplyAfterOptions(sel, [0, 1, 2], 2, false);

        expect(sel.value).toBe("Edit Stage 1");
        expect(optionValues(sel)).not.toContain("SeedVR2");
    });
});
