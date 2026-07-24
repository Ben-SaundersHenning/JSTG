<script lang="ts" setup>

    import { ref, onMounted } from "vue";
    import { useForm } from "vee-validate";
    import { toTypedSchema } from "@vee-validate/zod";
    import { z } from "zod";
    import { invoke } from "@tauri-apps/api/core";

    // form schema
    const { errors, handleSubmit, setFieldError, defineField, setFieldValue } = useForm({
        validationSchema: toTypedSchema(z.object({
            savePath: z.string().trim().min(1),
        }))
    });

    // fields
    const [savePath, savePathAtrb] = defineField("savePath");

    const onSubmit = handleSubmit(onSuccess, onInvalidSubmit);

    function onSuccess(values) {

        invoke('verify_directory', { directory: values.savePath }).then((truthy) => {
            if(truthy) {
                invoke('update_config', { conf: values });
            }
            else {
                setFieldError('savePath', 'Not a valid path');
            }
        })

    }

    function onInvalidSubmit({ values, errors, results }) {
        console.log(errors);
    }

    onMounted(() => {

        invoke('get_config').then((init_config) => {
            setFieldValue('savePath', init_config.savePath);
        })
        .catch((e) => console.log(e));

    })

</script>

<template>
    <form @submit="onSubmit">

        <div class="inputs">
            <div class="path-input vertical-input">
                <p class="input-label">Document Save Path</p>
                <input style="width: auto" aria-label="Document Save Path" id="savepath-input" class="input-border" type="text" name="savepath" v-model="savePath" :="savePathAtrb"/>
                <span class="error">{{errors['savePath']}}</span>
            </div>
        </div>
        <div class="horizontal-input" style="justify-content: center; margin-top: 30px;">
            <button class="submit" type="submit">Save</button>
        </div>

    </form>
</template>

<style lang="scss" scoped>

    @use '../variables';

    .inputs {
        margin: 30px;
    }

</style>
